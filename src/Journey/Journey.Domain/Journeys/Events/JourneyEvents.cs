using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace CleanArchitecture.Domain.Journeys.Events;

public sealed record JourneyCreated(int JourneyId, string OwnerUserId, DateTimeOffset OccurredOn)
    : IDomainEvent;

public sealed record JourneyUpdated(int JourneyId, string OwnerUserId, DateTimeOffset OccurredOn)
    : IDomainEvent;

public sealed record JourneyDeleted(int JourneyId, string OwnerUserId, DateTimeOffset OccurredOn)
    : IDomainEvent;

public sealed record JourneyShared(int JourneyId, string ActorUserId, IReadOnlyList<string> RecipientUserIds) : INotification;
public sealed record JourneyUnshared(int JourneyId, string ActorUserId, IReadOnlyList<string> RecipientUserIds) : INotification;