using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Users.Queries;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Users.Handlers;

public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedList<UserDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    private static readonly Dictionary<string, Expression<Func<User, object>>> SortExpressions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = u => u.Id!,
            ["Email"] = u => u.Email!,
            ["Name"] = u => u.Name!,
            ["Status"] = u => u.Status,
            ["CreatedAt"] = u => u.CreatedAt,
            ["LastSeenAt"] = u => u.LastSeenAt
        };

    public GetUsersHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.EntitySet<User>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            query = query.Where(u => u.Email != null && u.Email.Contains(request.Email));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(u => u.Name != null && u.Name.Contains(request.Name));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(u => u.Status == request.Status.Value);
        }

        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= request.CreatedTo.Value);
        }

        var sortKey = SortExpressions.TryGetValue(request.OrderBy ?? "", out var expression)
            ? expression
            : SortExpressions["CreatedAt"];

        var isDescending = string.Equals(request.Direction, "desc", StringComparison.OrdinalIgnoreCase);
        query = isDescending ? query.OrderByDescending(sortKey) : query.OrderBy(sortKey);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<UserDto>(items, page, pageSize, totalCount);
    }
}


