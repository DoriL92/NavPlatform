using System.Security.Claims;
using System.Text.RegularExpressions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Journeys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Journey.Api.Realtime;

[Authorize(Policy = "AuthedUser")]
public sealed class JourneyHub : Hub
{
    private readonly IPresenceTracker _presence;
    private readonly IApplicationDbContext _db;

    public JourneyHub(IPresenceTracker presence, IApplicationDbContext db)
        => (_presence, _db) = (presence, db);

    public static string GroupFor(int journeyId) => $"fav-{journeyId}";

    private string CurrentUserId =>
        Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
        Context.User?.FindFirstValue("sub") ??
        throw new HubException("User ID not found in claims");

    public override async Task OnConnectedAsync()
    {
        var uid = CurrentUserId;
        _presence.Add(uid, Context.ConnectionId);

        var favIds = await _db.EntitySet<JourneyFavorite>()
            .Where(f => f.UserId == uid)
            .Select(f => f.JourneyId)
            .ToListAsync(Context.ConnectionAborted);

        foreach (var jid in favIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(jid));

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var uid = CurrentUserId;
        _presence.Remove(uid, Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    // Optional instant join/leave after star toggle
    public Task SubscribeToJourney(int journeyId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(journeyId));

    public Task UnsubscribeFromJourney(int journeyId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupFor(journeyId));
}
