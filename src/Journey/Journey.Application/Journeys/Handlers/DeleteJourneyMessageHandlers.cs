using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class JourneyDeletedHandler : INotificationHandler<JourneyDeleted>
{
    private readonly ILogger<JourneyDeletedHandler> _log;
    public JourneyDeletedHandler(ILogger<JourneyDeletedHandler> log) => _log = log;

    public Task Handle(JourneyDeleted e, CancellationToken ct)
    {
        _log.LogInformation("Journey {Id} deleted at {At}", e.JourneyId, e.OccurredOn);
        return Task.CompletedTask;
    }
}