using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Application.Journeys.Services;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class UpdateJourneyHandler : IRequestHandler<UpdateJourneyCommand, Unit>
{

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IDateTime _clock;
    private readonly IDailyGoalCheckService _dailyGoalCheckService;

    public UpdateJourneyHandler(IApplicationDbContext db, ICurrentUser user, IDateTime clock, IDailyGoalCheckService dailyGoalCheckService)
        => (_db, _user, _clock, _dailyGoalCheckService) = (db, user, clock, dailyGoalCheckService);

    public async Task<Unit> Handle(UpdateJourneyCommand r, CancellationToken ct)
    {
        var j = await _db.EntitySet<JourneyEntity>().FirstOrDefaultAsync(x => x.Id == r.Id, ct)
                ?? throw new NotFoundException(nameof(JourneyEntity), r.Id);

        if (j.OwnerUserId != _user.UserId)
            throw new ForbiddenAccessException();

        if (!Enum.TryParse<TransportType>(r.TransportType, true, out var t))
            throw new ValidationException();

        j.Update(r.StartLocation, r.StartTime, r.ArrivalLocation, r.ArrivalTime,
                 t, DistanceKm.From(r.DistanceKm), _clock.Now);

        await _db.SaveChangesAsync(ct);
        
        await _dailyGoalCheckService.CheckAndTriggerDailyGoalAsync(_user.UserId!, r.Id , ct);
        
        return Unit.Value;
    }
}
