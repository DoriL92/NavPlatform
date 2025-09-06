using Microsoft.AspNetCore.Mvc;
using Notification.Api.Models;
using Notification.Api.Services;

namespace Notification.Api.Controllers;

[ApiController]
[Route("api/notify")]
public sealed class NotifyController : ControllerBase
{
    private readonly IEmailSender _sender;
    public NotifyController(IEmailSender sender) => _sender = sender;

    [HttpPost("journey-update")]
    public async Task<IActionResult> JourneyUpdate([FromBody] JourneyUpdateEmailRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email)) return NoContent();
        await _sender.SendJourneyUpdateAsync(req.Email, req.JourneyId, req.Kind, ct);
        return Accepted();
    }

}


