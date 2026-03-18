using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Domain.Entities.Gastronomia;

namespace arroyoSeco.Application.Features.Gastronomia.Commands.Crear;

public class CrearMenuCommand
{
    public int EstablecimientoId { get; set; }
    public string Nombre { get; set; } = null!;
}

public class CrearMenuCommandHandler
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _current;

    public CrearMenuCommandHandler(IAppDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<int> Handle(CrearMenuCommand request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("Nombre requerido");

        var est = await _context.Establecimientos.FirstOrDefaultAsync(e => e.Id == request.EstablecimientoId, ct);
        if (est == null) throw new InvalidOperationException("Establecimiento no encontrado");
        if (est.OferenteId != _current.UserId)
            throw new InvalidOperationException("Acceso denegado");

        var m = new Menu { EstablecimientoId = est.Id, Nombre = request.Nombre.Trim() };
        _context.Menus.Add(m);
        await _context.SaveChangesAsync(ct);
        return m.Id;
    }
}
