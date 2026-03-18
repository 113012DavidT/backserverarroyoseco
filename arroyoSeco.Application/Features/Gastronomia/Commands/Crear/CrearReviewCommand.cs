using System;
using System.Threading;
using System.Threading.Tasks;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Domain.Entities.Gastronomia;

namespace arroyoSeco.Application.Features.Gastronomia.Commands.Crear
{
    public class CrearReviewCommand
    {
        public int EstablecimientoId { get; set; }
        public string? UsuarioId { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public int Puntuacion { get; set; }
    }

    public class CrearReviewCommandHandler
    {
        private readonly IAppDbContext _context;

        public CrearReviewCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearReviewCommand request, CancellationToken ct = default)
        {
            var review = new Review
            {
                EstablecimientoId = request.EstablecimientoId,
                UsuarioId = request.UsuarioId ?? string.Empty,
                Comentario = request.Comentario,
                Puntuacion = request.Puntuacion,
                Fecha = DateTime.UtcNow
            };
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync(ct);
            return review.Id;
        }
    }
}
