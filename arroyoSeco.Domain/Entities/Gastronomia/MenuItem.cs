namespace arroyoSeco.Domain.Entities.Gastronomia;

public class MenuItem
{
    public int Id { get; set; }
    public int MenuId { get; set; }
    public Menu? Menu { get; set; }

    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
}
