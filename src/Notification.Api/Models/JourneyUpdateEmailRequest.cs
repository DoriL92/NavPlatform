namespace Notification.Api.Models;

public sealed class JourneyUpdateEmailRequest
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public int JourneyId { get; set; }
    public string Kind { get; set; } = default!;
}
