using System;

namespace arroyoSeco.Domain.Entities.Gastronomia
{
    public class Review
    {
        public int Id { get; set; }
        public int EstablecimientoId { get; set; }
        public Establecimiento Establecimiento { get; set; }
        public string UsuarioId { get; set; } // FK a usuario
        public string Comentario { get; set; }
        public int Puntuacion { get; set; } // 1-5
        public DateTime Fecha { get; set; }
    }
}
