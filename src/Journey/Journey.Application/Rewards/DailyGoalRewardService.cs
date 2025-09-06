using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Rewards;
using CleanArchitecture.Domain.Journeys;
using Microsoft.EntityFrameworkCore;

public interface IDailyRewardService
{
    Task<(bool awarded, decimal total)> CheckAndAwardAsync(string userId, DateOnly dayUtc, CancellationToken ct = default);
}

public sealed class DailyRewardService(
    IApplicationDbContext db,
    IDailyGoalCalculator calc,
    IDateTime clock
) : IDailyRewardService
{
    public async Task<(bool, decimal)> CheckAndAwardAsync(string userId, DateOnly dayUtc, CancellationToken ct = default)
    {
        var start = new DateTimeOffset(dayUtc.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = start.AddDays(1);

        var already = await db.EntitySet<JourneyEntity>()
            .AnyAsync(j => j.OwnerUserId == userId &&
                           j.StartTime >= start && j.StartTime < end &&
                           j.IsDailyGoalAchieved, ct);
        if (already) return (false, 20.00m);

        var journeys = await db.EntitySet<JourneyEntity>()
            .Where(j => j.OwnerUserId == userId &&
                        j.StartTime >= start && j.StartTime < end &&
                        !j.IsDeleted)
            .OrderBy(j => j.StartTime).ThenBy(j => j.Id)
            .ToListAsync(ct);

        if (journeys.Count == 0) return (false, 0m);

        var trigger = calc.FindBadgeJourney(journeys, out var totalKm);
        if (trigger is null) return (false, totalKm);

        trigger.MarkDailyGoalAchieved(clock.Now, totalKm); 
        await db.SaveChangesAsync(ct);

        return (true, totalKm);
    }
}