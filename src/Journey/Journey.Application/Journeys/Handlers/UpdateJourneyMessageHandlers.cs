using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class JourneyUpdatedHandler : INotificationHandler<JourneyUpdated>
{
    private readonly ILogger<JourneyUpdatedHandler> _log;
    public JourneyUpdatedHandler(ILogger<JourneyUpdatedHandler> log) => _log = log;

    public Task Handle(JourneyUpdated e, CancellationToken ct)
    {
        _log.LogInformation("Journey {Id} updated at {At}", e.JourneyId, e.OccurredOn);
        return Task.CompletedTask;
    }
}
