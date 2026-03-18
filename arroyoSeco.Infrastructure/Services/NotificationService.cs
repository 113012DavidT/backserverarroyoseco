using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Domain.Entities.Notificaciones;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using arroyoSeco.Domain.Entities.Usuarios;

namespace arroyoSeco.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IAppDbContext _ctx;
    private readonly IEmailService _email;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IAppDbContext ctx,
        IEmailService email,
        UserManager<ApplicationUser> userManager,
        ILogger<NotificationService> logger)
    {
        _ctx = ctx;
        _email = email;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<int> PushAsync(
        string usuarioId,
        string titulo,
        string mensaje,
        string tipo,
        string? url = null,
        CancellationToken ct = default)
    {
        var n = new Notificacion
        {
            UsuarioId = usuarioId,
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = tipo,
            UrlAccion = url,
            Leida = false,
            Fecha = DateTime.UtcNow
        };

        _ctx.Notificaciones.Add(n);
        await _ctx.SaveChangesAsync(ct);

        // Obtener el email ANTES de lanzar la tarea de background (mientras UserManager aún está disponible)
        var user = await _userManager.FindByIdAsync(usuarioId);
        var userEmail = user?.Email;

        _logger.LogInformation($"Notificación creada [{n.Id}] para usuario {usuarioId}. Email: {userEmail ?? "NO DISPONIBLE"}");

        // Enviar correo de forma asíncrona (sin bloquear) - usar CancellationToken.None para que no se cancele con la request
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            _logger.LogInformation($"Lanzando tarea background para enviar email a {userEmail}");
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation($"[BACKGROUND] Iniciando envío de email a {userEmail} para notificación {n.Id}");
                    await _email.SendNotificationEmailAsync(
                        userEmail,
                        titulo,
                        mensaje,
                        url,
                        CancellationToken.None);
                    _logger.LogInformation($"[BACKGROUND] Email enviado exitosamente a {userEmail}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[BACKGROUND] Error enviando email para notificación {n.Id}");
                }
            }, CancellationToken.None);
        }
        else
        {
            _logger.LogWarning($"No se puede enviar email: usuario {usuarioId} no tiene email registrado");
        }

        return n.Id;
    }

    public async Task MarkAsReadAsync(int id, string usuarioId, CancellationToken ct = default)
    {
        var n = await _ctx.Notificaciones.FindAsync(new object[] { id }, ct);
        if (n is null || n.UsuarioId != usuarioId) return;

        n.Leida = true;
        await _ctx.SaveChangesAsync(ct);
    }
}