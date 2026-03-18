namespace arroyoSeco.Domain.Entities.Gastronomia;

public class Mesa
{
    public int Id { get; set; }
    public int EstablecimientoId { get; set; }
    public Establecimiento? Establecimiento { get; set; }

    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public bool Disponible { get; set; } = true;
}
