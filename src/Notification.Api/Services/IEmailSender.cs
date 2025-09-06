namespace Notification.Api.Services;

public interface IEmailSender
{
    Task SendJourneyUpdateAsync(string toEmail, int journeyId, string kind, CancellationToken ct);
}
