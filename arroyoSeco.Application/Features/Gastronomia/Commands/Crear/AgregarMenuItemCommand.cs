using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Domain.Entities.Gastronomia;

namespace arroyoSeco.Application.Features.Gastronomia.Commands.Crear;

public class AgregarMenuItemCommand
{
    public int MenuId { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
}

public class AgregarMenuItemCommandHandler
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _current;

    public AgregarMenuItemCommandHandler(IAppDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<int> Handle(AgregarMenuItemCommand request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("Nombre requerido");
        if (request.Precio < 0) throw new ArgumentException("Precio inválido");

        var menu = await _context.Menus.Include(m => m.Establecimiento).FirstOrDefaultAsync(m => m.Id == request.MenuId, ct);
        if (menu == null) throw new InvalidOperationException("Menu no encontrado");
        if (menu.Establecimiento?.OferenteId != _current.UserId)
            throw new InvalidOperationException("Acceso denegado");

        var item = new MenuItem { MenuId = menu.Id, Nombre = request.Nombre.Trim(), Descripcion = request.Descripcion, Precio = request.Precio };
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return item.Id;
    }
}
