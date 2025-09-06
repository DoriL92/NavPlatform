using Azure.Core;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Application.Journeys.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Journey.Api.Controllers;

[Authorize(Policy = "AuthedUser")]
[ApiController]
[Route("internal/journeys")]
public class InternalJourneysController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _cfg;

    public InternalJourneysController(IMediator mediator, IConfiguration cfg)
    {
        _mediator = mediator; _cfg = cfg;
    }

    [HttpPost("{id:int}/daily-goal")]
    public async Task<IActionResult> MarkDailyGoal(int id ,decimal km)
    {
        await _mediator.Send(new MarkDailyGoalAchievedCommand(id, DateTimeOffset.UtcNow, km));
        return NoContent();
    }
}