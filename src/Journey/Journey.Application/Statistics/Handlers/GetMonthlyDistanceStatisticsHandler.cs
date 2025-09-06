using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Statistics.Queries;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Statistics.Handlers;

public sealed class GetMonthlyDistanceStatisticsHandler 
    : IRequestHandler<GetMonthlyDistanceStatisticsQuery, PagedList<MonthlyDistanceDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    private static readonly Dictionary<string, Expression<Func<MonthlyDistance, object>>> SortExpressions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["UserId"] = x => x.UserId,
            ["TotalDistanceKm"] = x => x.TotalDistanceKm,
            ["Year"] = x => x.Year,
            ["Month"] = x => x.Month
        };

    public GetMonthlyDistanceStatisticsHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedList<MonthlyDistanceDto>> Handle(
        GetMonthlyDistanceStatisticsQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _db.EntitySet<MonthlyDistance>().AsNoTracking();

        var sortKey = SortExpressions.TryGetValue(request.OrderBy ?? "", out var expression) 
            ? expression 
            : SortExpressions["UserId"];
        
        query = query.OrderBy(sortKey);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .ProjectTo<MonthlyDistanceDto>(_mapper.ConfigurationProvider)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<MonthlyDistanceDto>(items, page, pageSize, totalCount);
    }
}

