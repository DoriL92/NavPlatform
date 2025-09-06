using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace CleanArchitecture.Domain.Journeys.Events;

public sealed record DailyGoalAchieved(
    int JourneyId,
    string OwnerUserId,
    DateOnly Day,
    decimal TotalKm,
    DateTimeOffset OccurredOn
) : IDomainEvent;



