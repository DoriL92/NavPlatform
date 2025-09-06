using Azure;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Application.Journeys.Queries;
using Journey.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Journey.Api.Controllers;

[Authorize(Policy = "AuthedUser")]
[ApiController]
[Route("api/[controller]")]
public class JourneysController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _cfg;
    public JourneysController(IMediator mediator, IConfiguration cfg)
    {

        _mediator = mediator;
        _cfg = cfg;
    }
    [HttpPost]
    public async Task<IResult> CreateJourney([FromBody] CreateJourneyCommand cmd)
    {
        var id = await _mediator.Send(cmd);
        return Results.Created($"/api/journeys/{id}", new { id });
    }

    [HttpGet("{id:int}")]
    public Task<JourneyDto> GetJourney(int id) => _mediator.Send(new GetJourneyQuery(id));

    [HttpGet]
    public async Task<ActionResult<PagedList<JourneyDto>>> List([FromQuery] ListJourneysQuery q)
    {
        var page = await _mediator.Send(q);
        Response.Headers["X-Total-Count"] = page.TotalCount.ToString();
        return Ok(page);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateJourney(int id, [FromBody] UpdateJourneyCommand cmd)
    {
        await _mediator.Send(cmd with { Id = id });
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteJourney(int id)
    {
        await _mediator.Send(new DeleteJourneyCommand(id));
        return NoContent();

    }
    [HttpPost("{id:int}/favorite")]
    public async Task<IActionResult> Favourite(int id)
    {
        await _mediator.Send(new FavouriteJourneyCommand(id));
        return NoContent();
    }

    [HttpDelete("{id:int}/favorite")]
    public async Task<IActionResult> Unfavourite(int id)
    {
        await _mediator.Send(new UnfavouriteJourneyCommand(id));
        return NoContent();
    }

    [HttpPost("{id:int}/share")]
    public async Task<IActionResult> ShareByEmail(int id, [FromBody] ShareByEmailRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new ShareByEmailCommand(id, body.Emails), ct);
        return Ok(new { success = result.Success, shareCount = result.ShareCount });
    }

    //[Authorize(Policy = "Journeys:Write")]
    [HttpDelete("{id:int}/share")]
    public async Task<IActionResult> UnshareByEmail([FromRoute] int id, [FromBody] ShareByEmailRequest body, CancellationToken ct)
    {
        await _mediator.Send(new UnshareByEmailCommand(id, body.Emails), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/public-link")]
    public async Task<ActionResult<object>> CreatePublicLink([FromRoute] int id, CancellationToken ct)
    {
        // handler returns token; build URL here so infra doesn’t need HttpContext
        var token = await _mediator.Send(new CreatePublicLinkCommand(id), ct);

        var baseUrl = $"{Request.Scheme}://localhost:4200";
        var url = $"{baseUrl}/api/journeys/{token}";
        return Ok(new { url });
    }

    [HttpDelete("{id:int}/public-link")]
    public async Task<IActionResult> RevokePublicLink([FromRoute] int id, CancellationToken ct)
    {
        await _mediator.Send(new RevokePublicLinkCommand(id), ct);
        return NoContent();
    }

    }
