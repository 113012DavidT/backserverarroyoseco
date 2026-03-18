using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using arroyoSeco.Application.Common.Interfaces;

public class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;

    public SmtpEmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword),
                EnableSsl = true
            };
            var mail = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);
            await Task.Run(() => client.Send(mail), ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendNotificationEmailAsync(string toEmail, string titulo, string mensaje, string? actionUrl = null, CancellationToken ct = default)
    {
        var body = $"<h2>{titulo}</h2><p>{mensaje}</p>";
        if (!string.IsNullOrEmpty(actionUrl))
            body += $"<a href='{actionUrl}'>Ver más</a>";

        return await SendEmailAsync(toEmail, titulo, body, ct);
    }
}
