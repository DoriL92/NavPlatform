using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Rewards;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Journeys.Services;

public interface IDailyGoalCheckService
{
    Task CheckAndTriggerDailyGoalAsync(string userId, int journeyId, CancellationToken cancellationToken = default);
}

public sealed class DailyGoalCheckService : IDailyGoalCheckService
{
    private readonly IApplicationDbContext _db;
    private readonly IDailyGoalCalculator _calculator;
    private readonly IDateTime _clock;
    private readonly IMediator _mediator;
    private readonly ILogger<DailyGoalCheckService> _logger;

    public DailyGoalCheckService(
        IApplicationDbContext db,
        IDailyGoalCalculator calculator,
        IDateTime clock,
        IMediator mediator,
        ILogger<DailyGoalCheckService> logger)
    {
        _db = db;
        _calculator = calculator;
        _clock = clock;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task CheckAndTriggerDailyGoalAsync(string userId, int journeyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var journey = await _db.EntitySet<JourneyEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == journeyId && j.OwnerUserId == userId, cancellationToken);

            if (journey == null)
            {
                _logger.LogWarning("Journey {JourneyId} not found for user {UserId}", journeyId, userId);
                return;
            }

            var dayUtc = DateOnly.FromDateTime(journey.StartTime.UtcDateTime);
            
            var alreadyAchieved = await _db.EntitySet<JourneyEntity>()
                .AnyAsync(j => j.OwnerUserId == userId &&
                               j.StartTime >= dayUtc.ToDateTime(TimeOnly.MinValue) &&
                               j.StartTime < dayUtc.AddDays(1).ToDateTime(TimeOnly.MinValue) &&
                               j.IsDailyGoalAchieved, cancellationToken);

            if (alreadyAchieved)
            {
                _logger.LogDebug("User {UserId} already achieved daily goal on {Day}", userId, dayUtc);
                return;
            }

            var startOfDay = new DateTimeOffset(dayUtc.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var endOfDay = startOfDay.AddDays(1);

            var journeysToday = await _db.EntitySet<JourneyEntity>()
                .Where(j => j.OwnerUserId == userId &&
                           j.StartTime >= startOfDay &&
                           j.StartTime < endOfDay &&
                           !j.IsDeleted)
                .OrderBy(j => j.StartTime).ThenBy(j => j.Id)
                .ToListAsync(cancellationToken);

            if (journeysToday.Count == 0)
            {
                _logger.LogDebug("No journeys found for user {UserId} on {Day}", userId, dayUtc);
                return;
            }

            var triggerJourney = _calculator.FindBadgeJourney(journeysToday, out var totalKm);
            
            if (triggerJourney != null)
            {
                _logger.LogInformation("Daily goal achieved for user {UserId} on {Day} with {TotalKm}km, triggered by journey {JourneyId}", 
                    userId, dayUtc, totalKm, triggerJourney.Id);

                var journeyToUpdate = await _db.EntitySet<JourneyEntity>()
                    .FirstOrDefaultAsync(j => j.Id == triggerJourney.Id, cancellationToken);

                if (journeyToUpdate != null)
                {
                    journeyToUpdate.MarkDailyGoalAchieved(_clock.Now, totalKm);
                    await _db.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("Successfully marked journey {JourneyId} as daily goal achieved for user {UserId}", 
                        triggerJourney.Id, userId);
                }
            }
            else
            {
                _logger.LogDebug("Daily goal not yet achieved for user {UserId} on {Day}. Current total: {TotalKm}km", 
                    userId, dayUtc, totalKm);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking daily goal achievement for user {UserId}, journey {JourneyId}", userId, journeyId);
            throw;
        }
    }
}
