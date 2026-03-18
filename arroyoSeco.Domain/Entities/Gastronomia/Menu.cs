using System.Collections.Generic;

namespace arroyoSeco.Domain.Entities.Gastronomia;

public class Menu
{
    public int Id { get; set; }
    public int EstablecimientoId { get; set; }
    public Establecimiento? Establecimiento { get; set; }

    public string Nombre { get; set; } = null!;
    public List<MenuItem> Items { get; set; } = new();
}
