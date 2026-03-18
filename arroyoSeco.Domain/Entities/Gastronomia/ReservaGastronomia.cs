using System;

namespace arroyoSeco.Domain.Entities.Gastronomia;

public class ReservaGastronomia
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = null!;
    public int EstablecimientoId { get; set; }
    public Establecimiento? Establecimiento { get; set; }
    public int? MesaId { get; set; }
    public Mesa? Mesa { get; set; }

    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public int NumeroPersonas { get; set; }
    public decimal Total { get; set; }
}
