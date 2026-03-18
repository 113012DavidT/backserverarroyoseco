using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Domain.Entities.Gastronomia;

namespace arroyoSeco.Application.Features.Gastronomia.Commands.Crear;

public class CrearEstablecimientoCommand
{
    public string Nombre { get; set; } = null!;
    public string Ubicacion { get; set; } = null!;
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public string? Direccion { get; set; }
    public string? Descripcion { get; set; }
    public string? FotoPrincipal { get; set; }
}

public class CrearEstablecimientoCommandHandler
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _current;

    public CrearEstablecimientoCommandHandler(IAppDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<int> Handle(CrearEstablecimientoCommand request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("Nombre requerido");
        if (string.IsNullOrWhiteSpace(request.Ubicacion))
            throw new ArgumentException("Ubicación requerida");

        var owner = await _context.Oferentes
            .FirstOrDefaultAsync(o => o.Id == _current.UserId, ct);
        if (owner == null)
            throw new InvalidOperationException("Oferente no encontrado para el usuario actual");

        var e = new Establecimiento
        {
            OferenteId = owner.Id,
            Nombre = request.Nombre.Trim(),
            Ubicacion = request.Ubicacion.Trim(),
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            Direccion = request.Direccion?.Trim(),
            Descripcion = request.Descripcion,
            FotoPrincipal = request.FotoPrincipal
        };

        _context.Establecimientos.Add(e);
        await _context.SaveChangesAsync(ct);
        return e.Id;
    }
}
