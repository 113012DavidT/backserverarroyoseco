using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Application.Features.Gastronomia.Commands.Crear;
using arroyoSeco.Domain.Entities.Usuarios;

namespace arroyoSeco.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservasGastronomiaController : ControllerBase
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly CrearReservaGastronomiaCommandHandler _crear;
    private readonly IEmailService _email;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReservasGastronomiaController(
        IAppDbContext db,
        ICurrentUserService current,
        CrearReservaGastronomiaCommandHandler crear,
        IEmailService email,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _current = current;
        _crear = crear;
        _email = email;
        _userManager = userManager;
    }

    // POST /api/ReservasGastronomia
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearReservaGastronomiaCommand cmd, CancellationToken ct)
    {
        // Validación explícita de autenticación
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { message = "Debes iniciar sesión para crear una reserva" });

        if (string.IsNullOrWhiteSpace(_current.UserId))
            return Unauthorized(new { message = "Usuario no identificado" });

        try
        {
            var id = await _crear.Handle(cmd, ct);
            var reserva = await _db.ReservasGastronomia
                .AsNoTracking()
                .Include(r => r.Establecimiento)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            
            if (reserva is null) 
                return Created(nameof(Crear), new { Id = id });
            
            return CreatedAtAction(nameof(GetByIdGastronomia), new { id = reserva.Id }, new 
            { 
                reserva.Id, 
                reserva.EstablecimientoId, 
                reserva.Fecha, 
                reserva.NumeroPersonas, 
                reserva.Estado 
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Error creando reserva", detalle = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Datos inválidos", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno", detalle = ex.Message });
        }
    }

    // GET /api/ReservasGastronomia/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdGastronomia(int id, CancellationToken ct)
    {
        var reserva = await _db.ReservasGastronomia
            .AsNoTracking()
            .Include(r => r.Establecimiento)
            .Include(r => r.Mesa)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        
        if (reserva is null) 
            return NotFound(new { message = "Reserva no encontrada" });
        
        return Ok(new
        {
            reserva.Id,
            reserva.EstablecimientoId,
            EstablecimientoNombre = reserva.Establecimiento?.Nombre,
            reserva.MesaId,
            MesaNumero = reserva.Mesa?.Numero,
            reserva.UsuarioId,
            reserva.Fecha,
            reserva.NumeroPersonas,
            reserva.Estado,
            reserva.Total
        });
    }

    // GET /api/ReservasGastronomia/activas
    [HttpGet("activas")]
    public async Task<ActionResult> GetReservasActivas(CancellationToken ct)
    {
        var userId = _current.UserId;
        var now = DateTime.UtcNow;

        // Si es cliente, obtener sus reservas activas
        if (User.IsInRole("Cliente"))
        {
            var reservas = await _db.ReservasGastronomia
                .Where(r => r.UsuarioId == userId && 
                           (r.Estado == "Pendiente" || r.Estado == "Confirmada") &&
                           r.Fecha >= now)
                .Include(r => r.Establecimiento)
                .Include(r => r.Mesa)
                .AsNoTracking()
                .OrderBy(r => r.Fecha)
                .ToListAsync(ct);

            return Ok(reservas.Select(r => new
            {
                r.Id,
                r.EstablecimientoId,
                EstablecimientoNombre = r.Establecimiento?.Nombre,
                r.MesaId,
                MesaNumero = r.Mesa?.Numero,
                r.UsuarioId,
                r.Fecha,
                r.NumeroPersonas,
                r.Estado,
                r.Total
            }));
        }

        // Si es oferente, obtener reservas activas de sus establecimientos
        if (User.IsInRole("Oferente"))
        {
            var reservas = await _db.ReservasGastronomia
                .Where(r => r.Establecimiento!.OferenteId == userId &&
                           (r.Estado == "Pendiente" || r.Estado == "Confirmada") &&
                           r.Fecha >= now)
                .Include(r => r.Establecimiento)
                .Include(r => r.Mesa)
                .AsNoTracking()
                .OrderBy(r => r.Fecha)
                .ToListAsync(ct);

            return Ok(reservas.Select(r => new
            {
                r.Id,
                r.EstablecimientoId,
                EstablecimientoNombre = r.Establecimiento?.Nombre,
                r.MesaId,
                MesaNumero = r.Mesa?.Numero,
                r.UsuarioId,
                r.Fecha,
                r.NumeroPersonas,
                r.Estado,
                r.Total
            }));
        }

        return Ok(Array.Empty<object>());
    }

    // GET /api/ReservasGastronomia/historial
    [HttpGet("historial")]
    public async Task<ActionResult> GetHistorial(CancellationToken ct)
    {
        var userId = _current.UserId;
        var now = DateTime.UtcNow;

        if (User.IsInRole("Cliente"))
        {
            var reservas = await _db.ReservasGastronomia
                .Where(r => r.UsuarioId == userId && 
                           (r.Fecha < now || r.Estado == "Cancelada" || r.Estado == "Completada"))
                .Include(r => r.Establecimiento)
                .Include(r => r.Mesa)
                .AsNoTracking()
                .OrderByDescending(r => r.Fecha)
                .ToListAsync(ct);

            return Ok(reservas.Select(r => new
            {
                r.Id,
                r.EstablecimientoId,
                EstablecimientoNombre = r.Establecimiento?.Nombre,
                r.MesaId,
                MesaNumero = r.Mesa?.Numero,
                r.UsuarioId,
                r.Fecha,
                r.NumeroPersonas,
                r.Estado,
                r.Total
            }));
        }

        if (User.IsInRole("Oferente"))
        {
            var reservas = await _db.ReservasGastronomia
                .Where(r => r.Establecimiento!.OferenteId == userId &&
                           (r.Fecha < now || r.Estado == "Cancelada" || r.Estado == "Completada"))
                .Include(r => r.Establecimiento)
                .Include(r => r.Mesa)
                .AsNoTracking()
                .OrderByDescending(r => r.Fecha)
                .ToListAsync(ct);

            return Ok(reservas.Select(r => new
            {
                r.Id,
                r.EstablecimientoId,
                EstablecimientoNombre = r.Establecimiento?.Nombre,
                r.MesaId,
                MesaNumero = r.Mesa?.Numero,
                r.UsuarioId,
                r.Fecha,
                r.NumeroPersonas,
                r.Estado,
                r.Total
            }));
        }

        return Ok(Array.Empty<object>());
    }

    // PATCH /api/ReservasGastronomia/{id}/estado
    [Authorize(Roles = "Admin,Oferente")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoReservaGastronomiaDto dto, CancellationToken ct)
    {
        var reserva = await _db.ReservasGastronomia
            .Include(r => r.Establecimiento)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reserva == null) return NotFound(new { message = "Reserva no encontrada" });

        // Verificar que el oferente sea dueño del establecimiento
        if (User.IsInRole("Oferente") && reserva.Establecimiento?.OferenteId != _current.UserId)
        {
            return Forbid();
        }

        reserva.Estado = dto.Estado;
        await _db.SaveChangesAsync(ct);

        // Enviar correo al cliente
        var cliente = await _userManager.FindByIdAsync(reserva.UsuarioId);
        if (cliente?.Email != null)
        {
            var asunto = "";
            var mensaje = "";
            var color = "";

            if (dto.Estado == "Confirmada")
            {
                asunto = "Tu reserva en gastronomía ha sido confirmada";
                mensaje = $"Tu reserva en {reserva.Establecimiento?.Nombre} para {reserva.NumeroPersonas} personas el {reserva.Fecha:dd/MM/yyyy HH:mm} ha sido confirmada.";
                color = "#27ae60";
            }
            else if (dto.Estado == "Cancelada")
            {
                asunto = "Tu reserva en gastronomía ha sido cancelada";
                mensaje = $"Tu reserva en {reserva.Establecimiento?.Nombre} ha sido cancelada.";
                color = "#e74c3c";
            }
            else if (dto.Estado == "Completada")
            {
                asunto = "Tu reserva en gastronomía ha sido completada";
                mensaje = $"¡Gracias por visitarnos en {reserva.Establecimiento?.Nombre}! Esperamos verte pronto.";
                color = "#3498db";
            }

            if (!string.IsNullOrEmpty(mensaje))
            {
                var correoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {color}; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #ecf0f1; padding: 20px; border-radius: 0 0 5px 5px; }}
        .details {{ background-color: #fff; padding: 15px; border-left: 4px solid {color}; margin: 15px 0; }}
        .details p {{ margin: 5px 0; }}
        .auto-email {{ background-color: #fff3cd; padding: 12px; border-left: 4px solid #ffc107; margin: 15px 0; font-size: 12px; color: #856404; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #7f8c8d; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{asunto}</h1>
        </div>
        <div class='content'>
            <p>Hola {cliente.UserName},</p>
            <p>{mensaje}</p>
            
            <div class='details'>
                <p><strong>Establecimiento:</strong> {reserva.Establecimiento?.Nombre}</p>
                <p><strong>Fecha:</strong> {reserva.Fecha:dd/MM/yyyy HH:mm}</p>
                <p><strong>Personas:</strong> {reserva.NumeroPersonas}</p>
                <p><strong>Total:</strong> ${reserva.Total:F2}</p>
            </div>
            
            <p>Si tienes dudas, contáctanos a través de nuestro sitio web.</p>
            
            <div class='auto-email'>
                <strong>⚠️ Nota:</strong> Este es un correo automático, por favor no contestes a este mensaje. No recibiremos tu respuesta. Si necesitas ayuda, contáctanos a través de nuestro sitio web.
            </div>
        </div>
        <div class='footer'>
            <p>© 2025 Arroyo Seco. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

                await _email.SendEmailAsync(cliente.Email, asunto, correoHtml, ct);
            }
        }

        return Ok(new { reserva.Id, reserva.Estado });
    }

    public record CambiarEstadoReservaGastronomiaDto(string Estado);
}
