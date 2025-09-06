using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class PublishDailyGoalAchieved(IRewardsBus bus)
    : INotificationHandler<DailyGoalAchieved>
{
    public Task Handle(DailyGoalAchieved e, CancellationToken ct) =>
           bus.PublishAsync("journey.dailygoal.achieved", new
           {
               journeyId = e.JourneyId,
               ownerUserId = e.OwnerUserId,
               dayUtc = e.Day.ToString("yyyy-MM-dd"), 
               totalKm = e.TotalKm,
               achievedAt = e.OccurredOn
           }, ct);
}