using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using AutoMapper;


namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class AdminJourneyFilterHandler
    : IRequestHandler<AdminJourneyFilterQuery, (IReadOnlyList<JourneyDto>, int)>
{
    public readonly IMapper _mapper;
    private readonly IApplicationDbContext _db;
    public AdminJourneyFilterHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    private static readonly Dictionary<string, Expression<Func<JourneyEntity, object>>> Sort =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["StartAt"] = j => j.StartTime,
            ["ArrivalAt"] = j => j.ArrivalTime,
            ["DistanceKm"] = j => j.DistanceKm,       
            ["OwnerUserId"] = j => j.OwnerUserId,
            ["CreatedAt"] = j => j.CreatedAt,
            ["UpdatedAt"] = j => j.UpdatedAt,
        };

    public async Task<(IReadOnlyList<JourneyDto>, int)> Handle(AdminJourneyFilterQuery r, CancellationToken ct)
    {
        var q = _db.EntitySet<JourneyEntity>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(r.UserId))
            q = q.Where(j => j.OwnerUserId == r.UserId);

        if (!(r.TransportType== TransportType.Any))
            q = q.Where(j => j.TransportType == r.TransportType); 

        if (r.StartDateFrom is not null) q = q.Where(j => j.StartTime >= r.StartDateFrom);
        if (r.StartDateTo is not null) q = q.Where(j => j.StartTime <= r.StartDateTo);
        if (r.ArrivalDateFrom is not null) q = q.Where(j => j.ArrivalTime >= r.ArrivalDateFrom);
        if (r.ArrivalDateTo is not null) q = q.Where(j => j.ArrivalTime <= r.ArrivalDateTo);

        if (r.MinDistance is not null) q = q.Where(j => j.DistanceKm >= r.MinDistance);
        if (r.MaxDistance is not null) q = q.Where(j => j.DistanceKm <= r.MaxDistance);

        var total = await q.CountAsync(ct);

        var key = Sort.TryGetValue(r.OrderBy ?? "", out var k) ? k : Sort["CreatedAt"];
        var desc = string.Equals(r.Direction, "desc", StringComparison.OrdinalIgnoreCase);
        q = desc ? q.OrderByDescending(key) : q.OrderBy(key);

        var page = Math.Max(1, r.Page);
        var size = Math.Clamp(r.PageSize, 1, 200);

        var items = await q
      .ProjectTo<JourneyDto>(_mapper.ConfigurationProvider)
      .Skip((page - 1) * size)
      .Take(size)
      .ToListAsync(ct);

        return (items, total);
    }
}
