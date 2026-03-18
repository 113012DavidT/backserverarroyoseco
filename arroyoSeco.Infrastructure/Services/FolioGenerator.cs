using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace arroyoSeco.Infrastructure.Services;

public class FolioGenerator : IFolioGenerator
{
    private readonly AppDbContext _ctx;
    public FolioGenerator(AppDbContext ctx) => _ctx = ctx;

    public async Task<string> NextReservaFolioAsync(CancellationToken ct = default)
    {
        // Estrategia: Generar folio con GUID para garantizar unicidad y evitar conflictos de concurrencia
        // Formato: RES-2025-{random 8 chars}
        var year = DateTime.UtcNow.Year;
        var guid = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        var folio = $"RES-{year}-{guid}";
        
        // Verificar que no exista (paranoia, pero GUID es prácticamente único)
        var maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            var exists = await _ctx.Reservas.AnyAsync(r => r.Folio == folio, ct);
            if (!exists)
                return folio;
            
            // Si por algún milagro existe, generar otro
            guid = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            folio = $"RES-{year}-{guid}";
        }
        
        throw new InvalidOperationException("No se pudo generar un folio único después de 3 intentos");
    }
}