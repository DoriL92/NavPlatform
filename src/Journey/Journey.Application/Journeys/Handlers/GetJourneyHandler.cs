using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Application.Journeys.Queries;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Handlers;
public class GetJourneyHandler : IRequestHandler<GetJourneyQuery, JourneyDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IMapper _map;
    public GetJourneyHandler(IApplicationDbContext db, ICurrentUser user, IMapper map)
        => (_db, _user, _map) = (db, user, map);

    public async Task<JourneyDto> Handle(GetJourneyQuery r, CancellationToken ct)
    {
        var me = _user.UserId!;

        var dto = await _db.EntitySet<JourneyEntity>()
            .Where(j => j.Id == r.Id && !j.IsDeleted)
            .Select(j => new JourneyDto
            {
                Id = j.Id,
                StartLocation = j.StartLocation,
                StartTime = j.StartTime,
                ArrivalLocation = j.ArrivalLocation,
                ArrivalTime = j.ArrivalTime,
                TransportType = j.TransportType.ToString(),
                DistanceKm = j.DistanceKm.Value,
                IsDailyGoalAchieved = j.IsDailyGoalAchieved,

                IsOwnedByMe = j.OwnerUserId == me,
                IsShared = j.Shares.Any(s => s.TargetUserId == me && s.RevokedAt == null)
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(JourneyEntity), r.Id);

        return dto;
    }
}
