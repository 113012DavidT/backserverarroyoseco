using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using arroyoSeco.Application.Common.Interfaces;

namespace arroyoSeco.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Intento de enviar correo sin destinatario");
            return false;
        }

        try
        {
            _logger.LogInformation($"Iniciando envío de correo a {toEmail} - Asunto: {subject}");

            if (string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(_options.SmtpUsername) || string.IsNullOrWhiteSpace(_options.SmtpPassword))
            {
                _logger.LogError("Configuración SMTP incompleta");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                _logger.LogError("Email remitente no está configurado (FromEmail vacío)");
                return false;
            }

            using var smtp = new System.Net.Mail.SmtpClient(_options.SmtpHost, _options.SmtpPort);
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);

            var mail = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await smtp.SendMailAsync(mail);
            _logger.LogInformation($"✅ Correo enviado exitosamente a {toEmail}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Excepción enviando correo a {toEmail}");
            return false;
        }
    }

    public async Task<bool> SendNotificationEmailAsync(
        string toEmail,
        string titulo,
        string mensaje,
        string? actionUrl = null,
        CancellationToken ct = default)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2c3e50; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #ecf0f1; padding: 20px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; background-color: #27ae60; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #7f8c8d; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{titulo}</h1>
        </div>
        <div class='content'>
            <p>{mensaje}</p>
            {(string.IsNullOrWhiteSpace(actionUrl) ? "" : $"<a href='{actionUrl}' class='button'>Ver más</a>")}
        </div>
        <div class='footer'>
            <p>© 2025 Arroyo Seco. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, titulo, htmlBody, ct);
    }
}

