using System.Threading;
using System.Threading.Tasks;

namespace arroyoSeco.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default);

    Task<bool> SendNotificationEmailAsync(
        string toEmail,
        string titulo,
        string mensaje,
        string? actionUrl = null,
        CancellationToken ct = default);
}
