using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Application.Journeys.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Journey.Api.Controllers;

[ApiController]
[Route("api/journeys")]
public class PublicController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{token}")]
    public async Task<ActionResult<JourneyDto>> GetPublicJourney(string token, CancellationToken ct)
    {
        try
        {
            var journey = await _mediator.Send(new GetPublicJourneyQuery(token), ct);
            return Ok(journey);
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "Journey not found or link has been revoked" });
        }
    }
}
