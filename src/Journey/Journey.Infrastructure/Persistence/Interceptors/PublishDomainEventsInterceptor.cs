using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitecture.Infrastructure.Persistence.Interceptors;
public sealed class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private readonly List<IDomainEvent> _captured = new();

    public PublishDomainEventsInterceptor(IMediator mediator) => _mediator = mediator;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is { } ctx)
        {
            var newEvents = ctx.ChangeTracker
                .Entries<IHasDomainEvents>()
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            if (newEvents.Count > 0)
                _captured.AddRange(newEvents);
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        if (_captured.Count > 0)
        {
            foreach (var domainEvent in _captured)
                await _mediator.Publish(domainEvent, ct);

            // Clear from all tracked aggregates
            if (eventData.Context is { } ctx)
            {
                foreach (var entity in ctx.ChangeTracker.Entries<IHasDomainEvents>())
                    entity.Entity.ClearDomainEvents();
            }

            _captured.Clear();
        }

        return await base.SavedChangesAsync(eventData, result, ct);
    }
}