using System.Net;
using System.Net.Mail;

namespace Notification.Api.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

    public async Task SendJourneyUpdateAsync(string toEmail, int journeyId, string kind, CancellationToken ct)
    {
        using var smtp = new SmtpClient(_cfg["Smtp:Host"], int.Parse(_cfg["Smtp:Port"]!))
        {
            EnableSsl = bool.Parse(_cfg["Smtp:Ssl"] ?? "true"),
            Credentials = new NetworkCredential(_cfg["Smtp:User"], _cfg["Smtp:Pass"])
        };

        using var msg = new MailMessage(_cfg["Smtp:From"]!, toEmail)
        {
            Subject = $"Journey {kind}",
            Body = $"There was an update ({kind}) for journey #{journeyId}.",
            IsBodyHtml = false
        };
        await smtp.SendMailAsync(msg, ct);
    }
}