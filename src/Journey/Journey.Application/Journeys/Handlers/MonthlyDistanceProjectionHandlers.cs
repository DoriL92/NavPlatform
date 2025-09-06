using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Journeys.Handlers;

public sealed class JourneyCreatedProjectionHandler : INotificationHandler<JourneyCreated>
{
    private readonly IApplicationDbContext _db;
    private readonly IMonthlyDistanceProjector _projector;
    private readonly ILogger<JourneyCreatedProjectionHandler> _logger;

    public JourneyCreatedProjectionHandler(
        IApplicationDbContext db,
        IMonthlyDistanceProjector projector,
        ILogger<JourneyCreatedProjectionHandler> logger)
    {
        _db = db;
        _projector = projector;
        _logger = logger;
    }

    public async Task Handle(JourneyCreated notification, CancellationToken cancellationToken)
    {
        try
        {
            var journey = await _db.EntitySet<JourneyEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == notification.JourneyId, cancellationToken);

            if (journey != null)
            {
                await _projector.ApplyCreatedAsync(journey, cancellationToken);
                _logger.LogInformation("Updated monthly distance projection for created journey {JourneyId}", notification.JourneyId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update monthly distance projection for created journey {JourneyId}", notification.JourneyId);
            throw;
        }
    }
}

public sealed class JourneyUpdatedProjectionHandler : INotificationHandler<JourneyUpdated>
{
    private readonly IApplicationDbContext _db;
    private readonly IMonthlyDistanceProjector _projector;
    private readonly ILogger<JourneyUpdatedProjectionHandler> _logger;

    public JourneyUpdatedProjectionHandler(
        IApplicationDbContext db,
        IMonthlyDistanceProjector projector,
        ILogger<JourneyUpdatedProjectionHandler> logger)
    {
        _db = db;
        _projector = projector;
        _logger = logger;
    }

    public async Task Handle(JourneyUpdated notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogWarning("Journey updated projection handling needs before/after state tracking for journey {JourneyId}", notification.JourneyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update monthly distance projection for updated journey {JourneyId}", notification.JourneyId);
            throw;
        }
    }
}

public sealed class JourneyDeletedProjectionHandler : INotificationHandler<JourneyDeleted>
{
    private readonly IApplicationDbContext _db;
    private readonly IMonthlyDistanceProjector _projector;
    private readonly ILogger<JourneyDeletedProjectionHandler> _logger;

    public JourneyDeletedProjectionHandler(
        IApplicationDbContext db,
        IMonthlyDistanceProjector projector,
        ILogger<JourneyDeletedProjectionHandler> logger)
    {
        _db = db;
        _projector = projector;
        _logger = logger;
    }

    public async Task Handle(JourneyDeleted notification, CancellationToken cancellationToken)
    {
        try
        {
            
            _logger.LogWarning("Journey deleted projection handling needs pre-delete journey data for journey {JourneyId}", notification.JourneyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update monthly distance projection for deleted journey {JourneyId}", notification.JourneyId);
            throw;
        }
    }
}
