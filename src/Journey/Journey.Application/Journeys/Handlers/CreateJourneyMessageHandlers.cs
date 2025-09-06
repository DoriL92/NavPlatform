using CleanArchitecture.Domain.Journeys.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed class JourneyCreatedHandler : INotificationHandler<JourneyCreated>
{
    private readonly ILogger<JourneyCreatedHandler> _log;
    public JourneyCreatedHandler(ILogger<JourneyCreatedHandler> log) => _log = log;

    public Task Handle(JourneyCreated e, CancellationToken ct)
    {
        _log.LogInformation("Journey {Id} created by {User} at {At}", e.JourneyId, e.OwnerUserId, e.OccurredOn);
        return Task.CompletedTask;
    }
}