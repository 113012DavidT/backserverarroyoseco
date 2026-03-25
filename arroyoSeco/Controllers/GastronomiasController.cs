using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Application.Features.Gastronomia.Commands.Crear;
using EstablecimientoEntity = arroyoSeco.Domain.Entities.Gastronomia.Establecimiento;

namespace arroyoSeco.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GastronomiasController : ControllerBase
{
    private const string NeuronaBaseUrl = "http://34.51.58.191:5000";

    private readonly IAppDbContext _db;
    private readonly CrearEstablecimientoCommandHandler _crear;
    private readonly CrearMenuCommandHandler _crearMenu;
    private readonly AgregarMenuItemCommandHandler _agregarItem;
    private readonly CrearMesaCommandHandler _crearMesa;
    private readonly CrearReservaGastronomiaCommandHandler _crearReserva;
    private readonly ICurrentUserService _current;

    public GastronomiasController(
        IAppDbContext db,
        CrearEstablecimientoCommandHandler crear,
        CrearMenuCommandHandler crearMenu,
        AgregarMenuItemCommandHandler agregarItem,
        CrearMesaCommandHandler crearMesa,
        CrearReservaGastronomiaCommandHandler crearReserva,
        ICurrentUserService current)
    {
        _db = db;
        _crear = crear;
        _crearMenu = crearMenu;
        _agregarItem = agregarItem;
        _crearMesa = crearMesa;
        _crearReserva = crearReserva;
        _current = current;
    }

    [Authorize]
    [HttpPost("{id:int}/reviews")]
    public async Task<ActionResult<int>> CrearReview(int id, [FromBody] CrearReviewCommand cmd, CancellationToken ct)
    {
        if (cmd.Puntuacion < 1 || cmd.Puntuacion > 5)
            return BadRequest(new { message = "puntuacion debe estar entre 1 y 5" });
        if (string.IsNullOrWhiteSpace(cmd.Comentario))
            return BadRequest(new { message = "comentario es obligatorio" });

        var exists = await _db.Establecimientos.AnyAsync(e => e.Id == id, ct);
        if (!exists)
            return NotFound(new { message = "Establecimiento no encontrado" });

        cmd.EstablecimientoId = id;
        cmd.UsuarioId = _current.UserId;

        try
        {
            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var payload = new { puntuacion = cmd.Puntuacion, comentario = cmd.Comentario };
            var content = new System.Net.Http.StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://34.51.58.191:5000/predict", content, ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                var result = System.Text.Json.JsonDocument.Parse(json);
                int clase = result.RootElement.GetProperty("clase").GetInt32();
                cmd.Comentario += $" [Clasificación ML: {clase}]";
            }
        }
        catch
        {
            // Flask no disponible, guardar reseña igual sin clasificación
        }

        var handler = new CrearReviewCommandHandler(_db);
        var reviewId = await handler.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetReviews), new { id }, reviewId);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult> GetReviews(int id, CancellationToken ct)
    {
        var reviews = await _db.Reviews
            .Where(r => r.EstablecimientoId == id)
            .OrderByDescending(r => r.Fecha)
            .AsNoTracking()
            .ToListAsync(ct);
        return Ok(reviews);
    }

    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept")]
    public async Task<ActionResult<IEnumerable<EstablecimientoEntity>>> List(CancellationToken ct)
        => Ok(await _db.Establecimientos
            .Include(e => e.Menus)
            .Include(e => e.Mesas)
            .AsNoTracking()
            .ToListAsync(ct));

    [AllowAnonymous]
    [HttpGet("ranking")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept")]
    public async Task<ActionResult<IEnumerable<GastronomiaRankingDto>>> ListRanking(CancellationToken ct)
    {
        var ranked = await BuildRankingAsync(ct);
        var response = ranked.Select(x => new GastronomiaRankingDto(
            x.est.Id,
            x.est.Nombre,
            x.est.Ubicacion,
            x.est.Descripcion,
            x.est.FotoPrincipal,
            x.clase,
            x.confidence,
            x.fuente,
            x.est.Reviews.Count > 0 ? x.est.Reviews.Average(r => r.Puntuacion) : 0,
            x.est.Reviews.Count
        )).ToList();

        return Ok(response);
    }

    [Authorize(Roles = "Oferente")]
    [HttpGet("analytics")]
    public async Task<ActionResult<GastronomiaAnalyticsDto>> GetAnalytics(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_current.UserId))
            return Unauthorized();

        var establecimientoIds = _db.Establecimientos
            .AsNoTracking()
            .Where(e => e.OferenteId == _current.UserId)
            .Select(e => e.Id);

        var reviewsQuery = _db.Reviews
            .AsNoTracking()
            .Where(r => establecimientoIds.Contains(r.EstablecimientoId));

        var totalReviews = await reviewsQuery.CountAsync(ct);
        var ratingPromedio = totalReviews > 0
            ? await reviewsQuery.AverageAsync(r => (double)r.Puntuacion, ct)
            : 0;

        var distributionRaw = await reviewsQuery
            .GroupBy(r => r.Puntuacion)
            .Select(g => new { puntuacion = g.Key, total = g.Count() })
            .ToListAsync(ct);

        var distribution = Enumerable.Range(1, 5)
            .Select(p => new RatingDistributionDto(
                $"{p} estrella{(p == 1 ? string.Empty : "s")}",
                distributionRaw.FirstOrDefault(x => x.puntuacion == p)?.total ?? 0
            ))
            .ToList();

        var byEstablecimiento = await _db.Establecimientos
            .Include(e => e.Reviews)
            .AsNoTracking()
            .Where(e => e.OferenteId == _current.UserId && e.Reviews.Any())
            .Select(e => new EstablecimientoReviewStatsDto(
                e.Id,
                e.Nombre,
                e.Reviews.Average(r => (double)r.Puntuacion),
                e.Reviews.Count
            ))
            .ToListAsync(ct);

        var top5 = byEstablecimiento
            .OrderByDescending(x => x.Promedio)
            .ThenByDescending(x => x.TotalReviews)
            .Take(5)
            .ToList();

        var bottom5 = byEstablecimiento
            .OrderBy(x => x.Promedio)
            .ThenByDescending(x => x.TotalReviews)
            .Take(5)
            .ToList();

        var fromDate = DateTime.UtcNow.AddMonths(-5);
        var trendRaw = await reviewsQuery
            .Where(r => r.Fecha >= fromDate)
            .GroupBy(r => new { r.Fecha.Year, r.Fecha.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                total = g.Count()
            })
            .ToListAsync(ct);

        var trend = trendRaw
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .Select(x => new ReviewsTrendPointDto($"{x.Year}-{x.Month:D2}", x.total))
            .ToList();

        return Ok(new GastronomiaAnalyticsDto(
            totalReviews,
            ratingPromedio,
            distribution,
            top5,
            bottom5,
            trend
        ));
    }

    private async Task<List<(EstablecimientoEntity est, int clase, double confidence, string fuente)>> BuildRankingAsync(CancellationToken ct)
    {
        var establecimientos = await _db.Establecimientos
            .Include(e => e.Menus)
            .Include(e => e.Mesas)
            .Include(e => e.Reviews)
            .AsNoTracking()
            .ToListAsync(ct);

        var mlInput = establecimientos.Select(est =>
        {
            var avgPuntuacion = est.Reviews.Count > 0 ? est.Reviews.Average(r => r.Puntuacion) : 3.0;
            var lastComentario = est.Reviews.Count > 0
                ? est.Reviews.OrderByDescending(r => r.Fecha).First().Comentario
                : "sin reseñas";
            return new { puntuacion = avgPuntuacion, comentario = lastComentario };
        }).ToList();

        var fallback = establecimientos
            .Select(est =>
            {
                var avg = est.Reviews.Count > 0 ? est.Reviews.Average(r => r.Puntuacion) : 0;
                var clase = avg >= 4.0 ? 2 : (avg >= 2.5 ? 1 : 0);
                return (est, clase, confidence: avg / 5.0, fuente: "fallback");
            })
            .OrderByDescending(x => x.clase)
            .ThenByDescending(x => x.confidence)
            .ToList();

        try
        {
            using var http = new System.Net.Http.HttpClient();
            http.Timeout = TimeSpan.FromSeconds(5);
            var content = new System.Net.Http.StringContent(
                System.Text.Json.JsonSerializer.Serialize(mlInput),
                System.Text.Encoding.UTF8, "application/json");
            var response = await http.PostAsync($"{NeuronaBaseUrl}/score-batch", content, ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var scores = doc.RootElement.EnumerateArray()
                    .Select(el => (
                        clase: el.GetProperty("clase").GetInt32(),
                        confidence: el.GetProperty("confidence").GetDouble()
                    )).ToList();

                if (scores.Count == establecimientos.Count)
                {
                    return establecimientos
                    .Zip(scores, (e, s) => (est: e, s.clase, s.confidence, fuente: "ml"))
                    .OrderByDescending(x => x.clase)
                    .ThenByDescending(x => x.confidence)
                    .ToList();
                }
            }
        }
        catch
        {
            // Flask no disponible, usar fallback local
        }

        return fallback;
    }

    [Authorize(Roles = "Oferente")]
    [HttpGet("mios")]
    public async Task<ActionResult<IEnumerable<EstablecimientoEntity>>> GetMisEstablecimientos(CancellationToken ct)
    {
        var establecimientos = await _db.Establecimientos
            .Where(e => e.OferenteId == _current.UserId)
            .Include(e => e.Menus)
            .ThenInclude(m => m.Items)
            .Include(e => e.Mesas)
            .AsNoTracking()
            .ToListAsync(ct);

        return Ok(establecimientos);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstablecimientoEntity>> GetById(int id, CancellationToken ct)
    {
        var e = await _db.Establecimientos
            .Include(x => x.Menus)
            .ThenInclude(m => m.Items)
            .Include(x => x.Mesas)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [Authorize(Roles = "Oferente")]
    [HttpPost]
    public async Task<ActionResult<int>> Crear([FromBody] CrearEstablecimientoCommand cmd, CancellationToken ct)
    {
        var id = await _crear.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [Authorize(Roles = "Oferente")]
    [HttpPost("{id:int}/menus")]
    public async Task<ActionResult<int>> CrearMenu(int id, [FromBody] CrearMenuCommand cmd, CancellationToken ct)
    {
        cmd.EstablecimientoId = id;
        var mid = await _crearMenu.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, mid);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/menus")]
    public async Task<ActionResult> ListMenus(int id, CancellationToken ct)
    {
        var menus = await _db.Menus
            .Where(m => m.EstablecimientoId == id)
            .Include(m => m.Items)
            .AsNoTracking()
            .ToListAsync(ct);
        return Ok(menus);
    }

    [Authorize(Roles = "Oferente")]
    [HttpPost("{id:int}/menus/{menuId:int}/items")]
    public async Task<ActionResult<int>> AgregarItem(int id, int menuId, [FromBody] AgregarMenuItemCommand cmd, CancellationToken ct)
    {
        cmd.MenuId = menuId;
        var itemId = await _agregarItem.Handle(cmd, ct);
        return CreatedAtAction(nameof(ListMenus), new { id }, itemId);
    }

    [Authorize(Roles = "Oferente")]
    [HttpPost("{id:int}/mesas")]
    public async Task<ActionResult<int>> CrearMesa(int id, [FromBody] CrearMesaCommand cmd, CancellationToken ct)
    {
        cmd.EstablecimientoId = id;
        var mesaId = await _crearMesa.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, mesaId);
    }

    [Authorize(Roles = "Oferente")]
    [HttpPut("{id:int}/mesas/{mesaId:int}/disponible")]
    public async Task<IActionResult> SetDisponibilidad(int id, int mesaId, [FromBody] bool disponible, CancellationToken ct)
    {
        var mesa = await _db.Mesas.FirstOrDefaultAsync(m => m.Id == mesaId && m.EstablecimientoId == id, ct);
        if (mesa == null) return NotFound();
        if (mesa.Establecimiento?.OferenteId != _current.UserId) return Forbid();
        mesa.Disponible = disponible;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:int}/reservas")]
    public async Task<ActionResult<int>> CrearReserva(int id, [FromBody] CrearReservaGastronomiaCommand cmd, CancellationToken ct)
    {
        cmd.EstablecimientoId = id;
        var reservaId = await _crearReserva.Handle(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, reservaId);
    }

    [Authorize(Roles = "Oferente")]
    [HttpGet("{id:int}/reservas")]
    public async Task<ActionResult> ListReservas(int id, CancellationToken ct)
    {
        var est = await _db.Establecimientos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (est == null) return NotFound();
        if (est.OferenteId != _current.UserId) return Forbid();

        var reservas = await _db.ReservasGastronomia
            .Where(r => r.EstablecimientoId == id)
            .Include(r => r.Mesa)
            .AsNoTracking()
            .ToListAsync(ct);
        return Ok(reservas);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/disponibilidad")]
    public async Task<ActionResult> VerificarDisponibilidad(int id, [FromQuery] DateTime fecha, CancellationToken ct)
    {
        var mesas = await _db.Mesas
            .Where(m => m.EstablecimientoId == id && m.Disponible)
            .AsNoTracking()
            .ToListAsync(ct);
        return Ok(new { mesasDisponibles = mesas.Count, mesas });
    }

    [Authorize(Roles = "Oferente")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEstablecimientoRequest request, CancellationToken ct)
    {
        var est = await _db.Establecimientos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (est == null) return NotFound(new { message = "Establecimiento no encontrado" });
        if (est.OferenteId != _current.UserId) return Forbid();

        if (!string.IsNullOrWhiteSpace(request.Nombre))
            est.Nombre = request.Nombre;
        if (!string.IsNullOrWhiteSpace(request.Ubicacion))
            est.Ubicacion = request.Ubicacion;
        if (request.Latitud.HasValue)
            est.Latitud = request.Latitud;
        if (request.Longitud.HasValue)
            est.Longitud = request.Longitud;
        if (request.Direccion != null)
            est.Direccion = request.Direccion;
        if (request.Descripcion != null)
            est.Descripcion = request.Descripcion;
        if (!string.IsNullOrWhiteSpace(request.FotoPrincipal))
            est.FotoPrincipal = request.FotoPrincipal;

        await _db.SaveChangesAsync(ct);
        return Ok(est);
    }

    [Authorize(Roles = "Oferente")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var est = await _db.Establecimientos.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (est == null) return NotFound(new { message = "Establecimiento no encontrado" });
        if (est.OferenteId != _current.UserId) return Forbid();

        _db.Establecimientos.Remove(est);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Establecimiento eliminado correctamente" });
    }
}

public record UpdateEstablecimientoRequest(
    string? Nombre,
    string? Ubicacion,
    double? Latitud,
    double? Longitud,
    string? Direccion,
    string? Descripcion,
    string? FotoPrincipal
);

public record GastronomiaRankingDto(
    int Id,
    string Nombre,
    string Ubicacion,
    string? Descripcion,
    string? FotoPrincipal,
    int AiClase,
    double AiConfidence,
    string AiFuente,
    double RatingPromedio,
    int TotalReviews
);

public record RatingDistributionDto(
    string Etiqueta,
    int Valor
);

public record EstablecimientoReviewStatsDto(
    int EstablecimientoId,
    string Nombre,
    double Promedio,
    int TotalReviews
);

public record ReviewsTrendPointDto(
    string Etiqueta,
    int Valor
);

public record GastronomiaAnalyticsDto(
    int TotalResenas,
    double Promedio,
    List<RatingDistributionDto> DistribucionEstrellas,
    List<EstablecimientoReviewStatsDto> Top5,
    List<EstablecimientoReviewStatsDto> Bottom5,
    List<ReviewsTrendPointDto> TendenciaMensual
);
