using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class ForwardJourneyEventsToRewards :
    INotificationHandler<JourneyCreated>,
    INotificationHandler<JourneyUpdated>,
    INotificationHandler<JourneyDeleted>
{
    private readonly IRewardsBus _bus;
    public ForwardJourneyEventsToRewards(IRewardsBus bus) => _bus = bus;

    public Task Handle(JourneyCreated e, CancellationToken ct)
        => _bus.PublishRecalcAsync(e.OwnerUserId, e.OccurredOn, ct);

    public Task Handle(JourneyUpdated e, CancellationToken ct)
        => _bus.PublishRecalcAsync(e.OwnerUserId, e.OccurredOn, ct);

    public Task Handle(JourneyDeleted e, CancellationToken ct)
        => _bus.PublishRecalcAsync(e.OwnerUserId, e.OccurredOn, ct);
}
