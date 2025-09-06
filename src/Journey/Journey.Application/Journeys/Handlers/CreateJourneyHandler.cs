using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Application.Journeys.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Journeys;

using MediatR;

namespace CleanArchitecture.Application.Journeys.Handlers;
public class CreateJourneyHandler : IRequestHandler<CreateJourneyCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IDateTime _clock;
    private readonly IDailyGoalCheckService _dailyGoalCheckService;

    public CreateJourneyHandler(IApplicationDbContext db, ICurrentUser user, IDateTime clock, IDailyGoalCheckService dailyGoalCheckService)
        => (_db, _user, _clock, _dailyGoalCheckService) = (db, user, clock, dailyGoalCheckService);

    public async Task<int> Handle(CreateJourneyCommand r, CancellationToken ct)
    {
        if (!Enum.TryParse<TransportType>(r.TransportType, true, out var t))
            throw new ValidationException();

        var j = JourneyEntity.Create(_user.UserId!, _user.Name!, r.StartLocation, r.StartTime,
                               r.ArrivalLocation, r.ArrivalTime, t,
                               DistanceKm.From(r.DistanceKm), _clock.Now, r.IsDailyGoalAchieved);

        _db.EntitySet<JourneyEntity>().Add(j);
        await _db.SaveChangesAsync(ct);
        
        await _dailyGoalCheckService.CheckAndTriggerDailyGoalAsync(_user.UserId!, j.Id, ct);
        
        return j.Id;
    }
}
