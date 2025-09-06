using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Application.Statistics.Queries;
using CleanArchitecture.Application.Users.Commands;
using CleanArchitecture.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Journey.Api.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("statistics/monthly-distance")]
    public async Task<ActionResult<PagedList<MonthlyDistanceDto>>> GetMonthlyDistanceStatistics(
        [FromQuery] GetMonthlyDistanceStatisticsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    [HttpGet("journeys")]
    public async Task<ActionResult<PagedList<JourneyDto>>> GetJourneys(
        [FromQuery] AdminJourneyFilterQuery query, 
        CancellationToken cancellationToken)
    {
        var (items, total) = await _mediator.Send(query, cancellationToken);
        var result = new PagedList<JourneyDto>(items, query.Page, query.PageSize, total);
        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedList<UserDto>>> GetUsers(
        [FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        return Ok(result);
    }

    [HttpPatch("users/{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        string id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateUserStatusCommand(id, request.Status), cancellationToken);
        return NoContent();
    }
}

public class UpdateUserStatusRequest
{
    public string Status { get; set; } = default!;
}
