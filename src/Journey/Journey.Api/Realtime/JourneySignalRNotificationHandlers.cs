using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Journey.Api.Realtime;
using CleanArchitecture.Application.Abstractions.Messaging;

namespace Journey.Api.Realtime;

public sealed class JourneyUpdatedSignalRHandler : INotificationHandler<JourneyUpdated>
{
    private readonly IHubContext<JourneyHub> _hubContext;
    private readonly IApplicationDbContext _db;
    private readonly IPresenceTracker _presence;
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<JourneyUpdatedSignalRHandler> _logger;

    public JourneyUpdatedSignalRHandler(
        IHubContext<JourneyHub> hubContext,
        IApplicationDbContext db,
        IPresenceTracker presence,
        IEmailQueue emailQueue,
        ILogger<JourneyUpdatedSignalRHandler> logger)
    {
        _hubContext = hubContext;
        _db = db;
        _presence = presence;
        _emailQueue = emailQueue;
        _logger = logger;
    }

    public async Task Handle(JourneyUpdated notification, CancellationToken cancellationToken)
    {
        try
        {
            var favoritingUserIds = await _db.EntitySet<JourneyFavorite>()
                .Where(f => f.JourneyId == notification.JourneyId)
                .Select(f => f.UserId)
                .ToListAsync(cancellationToken);

            if (!favoritingUserIds.Any())
            {
                _logger.LogDebug("No users have favorited journey {JourneyId}, skipping notifications", notification.JourneyId);
                return;
            }

            var groupName = JourneyHub.GroupFor(notification.JourneyId);
            
            var journeyInfo = await _db.EntitySet<JourneyEntity>()
                .AsNoTracking()
                .Where(j => j.Id == notification.JourneyId)
                .Select(j => new
                {
                    j.Id,
                    j.StartLocation,
                    j.ArrivalLocation,
                    j.TransportType,
                    j.DistanceKm,
                    j.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            var userInfo = await _db.EntitySet<CleanArchitecture.Domain.Entities.User>()
                .AsNoTracking()
                .Where(u => u.Id == notification.OwnerUserId)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email
                })
                .FirstOrDefaultAsync(cancellationToken);

            await _hubContext.Clients.Group(groupName).SendAsync("JourneyUpdated", new
            {
                JourneyId = notification.JourneyId,
                OwnerUserId = notification.OwnerUserId,
                OccurredOn = notification.OccurredOn,
                JourneyInfo = journeyInfo,
                UserInfo = userInfo,
                Message = $"Journey from {journeyInfo?.StartLocation} to {journeyInfo?.ArrivalLocation} was updated by {userInfo?.Name ?? "Unknown User"}"
            }, cancellationToken);

            _logger.LogInformation("Sent JourneyUpdated SignalR notification for journey {JourneyId} to {UserCount} favoriting users", 
                notification.JourneyId, favoritingUserIds.Count);

            var offlineUsers = favoritingUserIds.Where(uid => !_presence.IsOnline(uid)).ToList();
            
            foreach (var userId in offlineUsers)
            {
                await _emailQueue.EnqueueJourneyUpdateAsync(userId, notification.JourneyId, "updated", cancellationToken);
                _logger.LogDebug("Queued email notification for offline user {UserId} about journey {JourneyId} update", 
                    userId, notification.JourneyId);
            }

            if (offlineUsers.Any())
            {
                _logger.LogInformation("Queued email notifications for {OfflineCount} offline users about journey {JourneyId} update", 
                    offlineUsers.Count, notification.JourneyId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for journey {JourneyId} update", notification.JourneyId);
            throw;
        }
    }
}

public sealed class JourneyDeletedSignalRHandler : INotificationHandler<JourneyDeleted>
{
    private readonly IHubContext<JourneyHub> _hubContext;
    private readonly IApplicationDbContext _db;
    private readonly IPresenceTracker _presence;
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<JourneyDeletedSignalRHandler> _logger;

    public JourneyDeletedSignalRHandler(
        IHubContext<JourneyHub> hubContext,
        IApplicationDbContext db,
        IPresenceTracker presence,
        IEmailQueue emailQueue,
        ILogger<JourneyDeletedSignalRHandler> logger)
    {
        _hubContext = hubContext;
        _db = db;
        _presence = presence;
        _emailQueue = emailQueue;
        _logger = logger;
    }

    public async Task Handle(JourneyDeleted notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get all users who have favorited this journey
            var favoritingUserIds = await _db.EntitySet<JourneyFavorite>()
                .Where(f => f.JourneyId == notification.JourneyId)
                .Select(f => f.UserId)
                .ToListAsync(cancellationToken);

            if (!favoritingUserIds.Any())
            {
                _logger.LogDebug("No users have favorited journey {JourneyId}, skipping notifications", notification.JourneyId);
                return;
            }

            var groupName = JourneyHub.GroupFor(notification.JourneyId);
            
            await _hubContext.Clients.Group(groupName).SendAsync("JourneyDeleted", new
            {
                JourneyId = notification.JourneyId,
                OwnerUserId = notification.OwnerUserId,
                OccurredOn = notification.OccurredOn
            }, cancellationToken);

            _logger.LogInformation("Sent JourneyDeleted SignalR notification for journey {JourneyId} to {UserCount} favoriting users", 
                notification.JourneyId, favoritingUserIds.Count);

            var offlineUsers = favoritingUserIds.Where(uid => !_presence.IsOnline(uid)).ToList();
            
            foreach (var userId in offlineUsers)
            {
                await _emailQueue.EnqueueJourneyUpdateAsync(userId, notification.JourneyId, "deleted", cancellationToken);
                _logger.LogDebug("Queued email notification for offline user {UserId} about journey {JourneyId} deletion", 
                    userId, notification.JourneyId);
            }

            if (offlineUsers.Any())
            {
                _logger.LogInformation("Queued email notifications for {OfflineCount} offline users about journey {JourneyId} deletion", 
                    offlineUsers.Count, notification.JourneyId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for journey {JourneyId} deletion", notification.JourneyId);
            throw;
        }
    }
}

public sealed class DailyGoalAchievedSignalRHandler : INotificationHandler<DailyGoalAchieved>
{
    private readonly IHubContext<JourneyHub> _hubContext;
    private readonly IApplicationDbContext _db;
    private readonly ILogger<DailyGoalAchievedSignalRHandler> _logger;

    public DailyGoalAchievedSignalRHandler(
        IHubContext<JourneyHub> hubContext,
        IApplicationDbContext db,
        ILogger<DailyGoalAchievedSignalRHandler> logger)
    {
        _hubContext = hubContext;
        _db = db;
        _logger = logger;
    }

    public async Task Handle(DailyGoalAchieved notification, CancellationToken cancellationToken)
    {
        try
        {
            var journeyInfo = await _db.EntitySet<JourneyEntity>()
                .AsNoTracking()
                .Where(j => j.Id == notification.JourneyId)
                .Select(j => new
                {
                    j.Id,
                    j.StartLocation,
                    j.ArrivalLocation,
                    j.TransportType,
                    j.DistanceKm,
                    j.StartTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            var userInfo = await _db.EntitySet<CleanArchitecture.Domain.Entities.User>()
                .AsNoTracking()
                .Where(u => u.Id == notification.OwnerUserId)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email
                })
                .FirstOrDefaultAsync(cancellationToken);

            await _hubContext.Clients.User(notification.OwnerUserId).SendAsync("DailyGoalAchieved", new
            {
                JourneyId = notification.JourneyId,
                OwnerUserId = notification.OwnerUserId,
                Day = notification.Day.ToString("yyyy-MM-dd"),
                TotalKm = notification.TotalKm,
                OccurredOn = notification.OccurredOn,
                JourneyInfo = journeyInfo,
                UserInfo = userInfo,
                Message = $"🎉 Congratulations! You've achieved your daily goal of 20km! Total distance: {notification.TotalKm}km"
            }, cancellationToken);

            _logger.LogInformation("Sent DailyGoalAchieved SignalR notification for user {UserId} on {Day} with {TotalKm}km", 
                notification.OwnerUserId, notification.Day, notification.TotalKm);

            var favoritingUserIds = await _db.EntitySet<JourneyFavorite>()
                .Where(f => f.JourneyId == notification.JourneyId && f.UserId != notification.OwnerUserId)
                .Select(f => f.UserId)
                .ToListAsync(cancellationToken);

            if (favoritingUserIds.Any())
            {
                var achievementMessage = $"{userInfo?.Name ?? "A user"} achieved their daily goal! Journey: {journeyInfo?.StartLocation} → {journeyInfo?.ArrivalLocation}";
                
                foreach (var userId in favoritingUserIds)
                {
                    await _hubContext.Clients.User(userId).SendAsync("DailyGoalAchieved", new
                    {
                        JourneyId = notification.JourneyId,
                        OwnerUserId = notification.OwnerUserId,
                        Day = notification.Day.ToString("yyyy-MM-dd"),
                        TotalKm = notification.TotalKm,
                        OccurredOn = notification.OccurredOn,
                        JourneyInfo = journeyInfo,
                        UserInfo = userInfo,
                        Message = achievementMessage,
                        IsOwnAchievement = false
                    }, cancellationToken);
                }

                _logger.LogInformation("Sent DailyGoalAchieved notifications to {UserCount} users who favorited journey {JourneyId}", 
                    favoritingUserIds.Count, notification.JourneyId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for daily goal achievement for user {UserId}", notification.OwnerUserId);
            throw;
        }
    }
}
