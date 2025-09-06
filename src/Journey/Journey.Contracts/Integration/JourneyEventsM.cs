using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journey.Contracts.Integration;
public record JourneyCreatedMessage(
    int JourneyId,
    string OwnerUserId,
    DateTimeOffset StartTime,
    decimal DistanceKm);

public record JourneyUpdatedMessage(
    int JourneyId,
    string OwnerUserId,
    DateTimeOffset StartTime,
    decimal DistanceKm);

public record JourneyDeletedMessage(
    int JourneyId,
    string OwnerUserId,
    DateTimeOffset StartTime);

public record DailyGoalAchievedMessage(
    string OwnerUserId,
    DateOnly DayUtc,
    int JourneyId,
    DateTimeOffset StartTime);
