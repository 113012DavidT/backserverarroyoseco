namespace arroyoSeco.Infrastructure.Services;

public class EmailOptions
{
    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = "Arroyo Seco";
}
