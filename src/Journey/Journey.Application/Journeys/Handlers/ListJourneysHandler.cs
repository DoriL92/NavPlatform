using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Application.Journeys.Queries;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Handlers;
public class ListJourneysHandler
    : IRequestHandler<ListJourneysQuery, PagedList<JourneyDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IMapper _map;

    public ListJourneysHandler(IApplicationDbContext db, ICurrentUser user, IMapper map)
        => (_db, _user, _map) = (db, user, map);

    public async Task<PagedList<JourneyDto>> Handle(ListJourneysQuery r, CancellationToken ct)
    {
        var userId = _user.UserId;

        var favIdsQ = _db.EntitySet<JourneyFavorite>()
            .Where(f => f.UserId == userId)
            .Select(f => f.JourneyId);

        var q = _db.EntitySet<JourneyEntity>()
            .Where(j => !j.IsDeleted &&
                       (j.OwnerUserId == userId || favIdsQ.Contains(j.Id) ||
                         j.Shares.Any(s => s.TargetUserId == userId && s.RevokedAt == null)))
            .Select(j => new
            {
                J = j,
                IsFav = favIdsQ.Contains(j.Id)
            })
            .OrderByDescending(x => x.J.StartTime)
            .Select(x => new JourneyDto
            {
                Id = x.J.Id,
                OwnerUserId = x.J.OwnerUserId,
                StartLocation = x.J.StartLocation,
                StartTime = x.J.StartTime,
                ArrivalLocation = x.J.ArrivalLocation,
                ArrivalTime = x.J.ArrivalTime,
                TransportType = x.J.TransportType.ToString(),
                DistanceKm = x.J.DistanceKm.Value,
                IsDailyGoalAchieved = x.J.IsDailyGoalAchieved, 
                IsFavorite = x.IsFav,
                IsOwnedByMe = x.J.OwnerUserId == userId,
                IsShared = x.J.Shares.Any(s => s.TargetUserId == userId && s.RevokedAt == null)
            });

        return await q.ToPagedListAsync(r.Page, r.PageSize, ct);
    }
}
