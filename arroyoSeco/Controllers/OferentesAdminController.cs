using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Domain.Entities.Solicitudes;
using arroyoSeco.Domain.Entities.Usuarios;
using UsuarioOferente = arroyoSeco.Domain.Entities.Usuarios.Oferente;

namespace arroyoSeco.Controllers;

[ApiController]
[Route("api/admin/oferentes")]
[Authorize(Roles = "Admin")]
public class OferentesAdminController : ControllerBase
{
    private readonly IAppDbContext _db;
    private readonly INotificationService _noti;
    private readonly IEmailService _email;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public OferentesAdminController(
        IAppDbContext db,
        INotificationService noti,
        IEmailService email,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _noti = noti;
        _email = email;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public record CrearUsuarioOferenteDto(string Email, string Password, string Nombre, int Tipo);

    [HttpPost("usuarios")]
    public async Task<IActionResult> CrearUsuarioOferente([FromBody] CrearUsuarioOferenteDto dto, CancellationToken ct)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null) return Conflict("Ya existe un usuario con ese email.");

        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true, RequiereCambioPassword = true };
        var res = await _userManager.CreateAsync(user, dto.Password);
        if (!res.Succeeded) return BadRequest(res.Errors);

        if (!await _roleManager.RoleExistsAsync("Oferente"))
            await _roleManager.CreateAsync(new IdentityRole("Oferente"));
        await _userManager.AddToRoleAsync(user, "Oferente");

        if (!await _db.Oferentes.AnyAsync(o => o.Id == user.Id, ct))
        {
            var o = new UsuarioOferente { Id = user.Id, Nombre = dto.Nombre, NumeroAlojamientos = 0, Tipo = (arroyoSeco.Domain.Entities.Enums.TipoOferente)dto.Tipo };
            _db.Oferentes.Add(o);
            await _db.SaveChangesAsync(ct);
        }

        var tipoTexto = GetTipoTexto(dto.Tipo);
        var correoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #ecf0f1; padding: 20px; border-radius: 0 0 5px 5px; }}
        .credentials {{ background-color: #fff; padding: 15px; border-left: 4px solid #27ae60; margin: 15px 0; }}
        .credentials p {{ margin: 5px 0; }}
        .auto-email {{ background-color: #fff3cd; padding: 12px; border-left: 4px solid #ffc107; margin: 15px 0; font-size: 12px; color: #856404; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #7f8c8d; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>¡Tu Cuenta de Oferente ha sido Creada!</h1>
        </div>
        <div class='content'>
            <p>Hola {dto.Nombre},</p>
            <p>Tu cuenta de oferente en <strong>Arroyo Seco</strong> para <strong>{tipoTexto}</strong> ha sido creada por un administrador.</p>
            <div class='credentials'>
                <p><strong>Email:</strong> {dto.Email}</p>
                <p><strong>Contraseña:</strong> {dto.Password}</p>
                <p><em>Por favor, cambia tu contraseña al iniciar sesión por primera vez.</em></p>
            </div>
            <p>Puedes acceder a tu panel de control en: <a href='https://arroyosecoservices.vercel.app/login'>Inicia sesión</a></p>
            <p>Si tienes dudas, contáctanos a través de nuestro sitio web.</p>
            <p>¡Esperamos trabajar contigo!</p>
            <div class='auto-email'>
                <strong>⚠️ Nota:</strong> Este es un correo automático, por favor no contestes a este mensaje.
            </div>
        </div>
        <div class='footer'>
            <p>© 2025 Arroyo Seco. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

        await _email.SendEmailAsync(dto.Email, "Tu Cuenta de Oferente ha sido Creada", correoHtml, ct);
        await _noti.PushAsync(user.Id, "Cuenta de Oferente creada",
            $"Tu cuenta de oferente para {tipoTexto} ha sido creada por un administrador. Hemos enviado tus credenciales al correo.", "Oferente", null, ct);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, new { user.Id, user.Email });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await _db.Oferentes.AsNoTracking().ToListAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var o = await _db.Oferentes.Include(x => x.Alojamientos).FirstOrDefaultAsync(x => x.Id == id, ct);
        return o is null ? NotFound() : Ok(o);
    }

    public record ActualizarOferenteDto(string? Nombre, string? Telefono, int? Tipo);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ActualizarOferenteDto dto, CancellationToken ct)
    {
        var o = await _db.Oferentes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return NotFound(new { message = "Oferente no encontrado" });

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            o.Nombre = dto.Nombre;

        if (dto.Tipo.HasValue)
            o.Tipo = (arroyoSeco.Domain.Entities.Enums.TipoOferente)dto.Tipo.Value;

        if (dto.Telefono != null)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.PhoneNumber = dto.Telefono;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return BadRequest(new { message = "Error al actualizar teléfono", errors = updateResult.Errors });
            }
        }

        await _db.SaveChangesAsync(ct);

        var userFinal = await _userManager.FindByIdAsync(id);
        return Ok(new
        {
            o.Id,
            o.Nombre,
            Tipo = (int)o.Tipo,
            o.Estado,
            Email = userFinal?.Email,
            Telefono = userFinal?.PhoneNumber
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var o = await _db.Oferentes.Include(x => x.Alojamientos).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return NotFound();
        if (o.Alojamientos?.Any() == true) return BadRequest("No se puede eliminar: tiene alojamientos asociados.");
        _db.Oferentes.Remove(o);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("solicitudes")]
    public async Task<IActionResult> ListSolicitudes([FromQuery] string? estatus, CancellationToken ct)
    {
        var q = _db.SolicitudesOferente.AsQueryable();
        if (!string.IsNullOrWhiteSpace(estatus)) q = q.Where(s => s.Estatus == estatus);
        var items = await q.OrderByDescending(s => s.FechaSolicitud).AsNoTracking().ToListAsync(ct);
        return Ok(items);
    }

    [HttpPost("solicitudes/{id:int}/aprobar")]
    public async Task<IActionResult> Aprobar(int id, CancellationToken ct)
    {
        var s = await _db.SolicitudesOferente.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound(new { message = "Solicitud no encontrada" });

        var email = string.IsNullOrWhiteSpace(s.Correo) ? $"oferente{id}@arroyoseco.com" : s.Correo.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        
        // Siempre generar contraseña temporal nueva
        string tempPass = "Temp" + Guid.NewGuid().ToString("N")[..8] + "!";

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = s.Telefono,
                RequiereCambioPassword = true
            };

            var res = await _userManager.CreateAsync(user, tempPass);
            if (!res.Succeeded) return BadRequest(res.Errors);

            if (!await _roleManager.RoleExistsAsync("Oferente"))
                await _roleManager.CreateAsync(new IdentityRole("Oferente"));
            await _userManager.AddToRoleAsync(user, "Oferente");
        }
        else
        {
            // ✅ Usuario ya existe: resetear contraseña con la nueva temporal
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, tempPass);
            if (!resetResult.Succeeded) return BadRequest(resetResult.Errors);

            user.RequiereCambioPassword = true;
            await _userManager.UpdateAsync(user);
        }

        if (!await _db.Oferentes.AnyAsync(o => o.Id == user.Id, ct))
        {
            _db.Oferentes.Add(new UsuarioOferente
            {
                Id = user.Id,
                Nombre = s.NombreNegocio,
                NumeroAlojamientos = 0,
                Tipo = s.TipoSolicitado,
                Estado = "Pendiente"
            });
        }

        s.Estatus = "Aprobada";
        s.FechaRespuesta = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var tipoTexto = GetTipoTexto((int)s.TipoSolicitado);
        var correoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #ecf0f1; padding: 20px; border-radius: 0 0 5px 5px; }}
        .credentials {{ background-color: #fff; padding: 15px; border-left: 4px solid #27ae60; margin: 15px 0; }}
        .credentials p {{ margin: 5px 0; }}
        .auto-email {{ background-color: #fff3cd; padding: 12px; border-left: 4px solid #ffc107; margin: 15px 0; font-size: 12px; color: #856404; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #7f8c8d; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>¡Bienvenido a Arroyo Seco!</h1>
        </div>
        <div class='content'>
            <p>Hola {s.NombreSolicitante},</p>
            <p>Tu solicitud para ser oferente de <strong>{tipoTexto}</strong> ha sido <strong style='color: #27ae60;'>APROBADA</strong>.</p>
            <div class='credentials'>
                <p><strong>Email:</strong> {email}</p>
                <p><strong>Contraseña temporal:</strong> {tempPass}</p>
                <p><em>Por favor, cambia tu contraseña al iniciar sesión por primera vez.</em></p>
            </div>
            <p>Puedes acceder a tu panel de control en: <a href='https://arroyosecoservices.vercel.app/login'>Inicia sesión</a></p>
            <p>Si tienes dudas, contáctanos a través de nuestro sitio web.</p>
            <p>¡Esperamos trabajar contigo!</p>
            <div class='auto-email'>
                <strong>⚠️ Nota:</strong> Este es un correo automático, por favor no contestes a este mensaje.
            </div>
        </div>
        <div class='footer'>
            <p>© 2025 Arroyo Seco. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

        await _email.SendEmailAsync(email, "Tu cuenta de Oferente ha sido aprobada", correoHtml, ct);
        await _noti.PushAsync(user.Id, "Solicitud aprobada",
            $"Tu solicitud para ser oferente de {tipoTexto} fue aprobada. Hemos enviado tus credenciales al correo.",
            "SolicitudOferente", null, ct);

        return Ok(new { id = user.Id, email = user.Email, tipo = s.TipoSolicitado, message = "Solicitud aprobada y correo enviado" });
    }

    [HttpPost("solicitudes/{id:int}/rechazar")]
    public async Task<IActionResult> Rechazar(int id, CancellationToken ct)
    {
        var s = await _db.SolicitudesOferente.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return NotFound(new { message = "Solicitud no encontrada" });

        s.Estatus = "Rechazada";
        s.FechaRespuesta = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var correoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #e74c3c; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #ecf0f1; padding: 20px; border-radius: 0 0 5px 5px; }}
        .auto-email {{ background-color: #fff3cd; padding: 12px; border-left: 4px solid #ffc107; margin: 15px 0; font-size: 12px; color: #856404; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #7f8c8d; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Solicitud de Oferente - Decisión</h1>
        </div>
        <div class='content'>
            <p>Hola {s.NombreSolicitante},</p>
            <p>Lamentablemente, tu solicitud para ser oferente en Arroyo Seco ha sido <strong style='color: #e74c3c;'>RECHAZADA</strong> en esta ocasión.</p>
            <p>Puedes volver a intentar en el futuro presentando una nueva solicitud.</p>
            <p>Si tienes preguntas, no dudes en contactarnos.</p>
            <div class='auto-email'>
                <strong>⚠️ Nota:</strong> Este es un correo automático, por favor no contestes a este mensaje.
            </div>
        </div>
        <div class='footer'>
            <p>© 2025 Arroyo Seco. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

        await _email.SendEmailAsync(s.Correo, "Tu solicitud de oferente ha sido rechazada", correoHtml, ct);
        return Ok(new { message = "Solicitud rechazada y correo enviado" });
    }

    public record CambiarEstadoDto(string Estado);

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(string id, [FromBody] CambiarEstadoDto dto, CancellationToken ct)
    {
        var oferente = await _db.Oferentes.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (oferente == null)
            return NotFound(new { message = "Oferente no encontrado" });

        oferente.Estado = dto.Estado;
        await _db.SaveChangesAsync(ct);

        return Ok(new { id = oferente.Id, estado = oferente.Estado });
    }

    private string GetTipoTexto(int tipo) => tipo switch
    {
        1 => "Alojamiento",
        2 => "Gastronomía",
        3 => "Ambos",
        _ => "Desconocido"
    };
}
