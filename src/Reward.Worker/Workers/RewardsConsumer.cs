using System.Text;
using System.Text.Json;
using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Infrastructure.Messaging;
using CleanArchitecture.Infrastructure.Persistence;
using Journey.Contracts.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public sealed class RewardsConsumer(
 ILogger<RewardsConsumer> log,
 IServiceProvider sp,
 IRewardsBus bus,      
 IDateTime clock)
 : BackgroundService
{
    private const decimal ThresholdKm = 20m;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return bus.SubscribeAsync<JourneyCreatedMessage>(
            topic: "journey.created",
            handler: OnJourneyCreated,
            stoppingToken);
    }

    private async Task OnJourneyCreated(JourneyCreatedMessage m, CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var j = await db.EntitySet<JourneyEntity>()
                        .FirstOrDefaultAsync(x => x.Id == m.JourneyId && !x.IsDeleted, ct);
        if (j is null) return;

        var ownerId = j.OwnerUserId;
        var dayUtc = DateOnly.FromDateTime(j.StartTime.UtcDateTime.Date);

        var already = await db.Set<DailyGoal>()
                              .AnyAsync(x => x.OwnerUserId == ownerId && x.DayUtc == dayUtc, ct);
        if (already) return;

        var totalKm = await db.EntitySet<JourneyEntity>()
            .Where(x => x.OwnerUserId == ownerId
                     && !x.IsDeleted
                     && x.StartTime.UtcDateTime.Date == j.StartTime.UtcDateTime.Date)
            .SumAsync(x => x.DistanceKm.Value, ct);

        if (totalKm < ThresholdKm) return;

        db.Set<DailyGoal>().Add(new DailyGoal
        {
            OwnerUserId = ownerId,
            DayUtc = dayUtc,
            AchievedJourneyId = j.Id,
            AchievedAtUtc = clock.Now.UtcDateTime
        });

        try
        {
            await db.SaveChangesAsync(ct); 
        }
        catch (DbUpdateException ex)
        {
            log.LogDebug(ex, "DailyGoal duplicate for {User}/{Day}", ownerId, dayUtc);
            return;
        }

        if (!j.IsDailyGoalAchieved)
        {
            j.MarkDailyGoalAchieved(clock.Now, totalKm);
            await db.SaveChangesAsync(ct);
        }

        await bus.PublishAsync("journey.dailygoal.achieved", new
        {
            journeyId = j.Id,
            ownerUserId = ownerId,
            dayUtc,
            totalKm,
            achievedAt = clock.Now
        }, ct);
    }
}