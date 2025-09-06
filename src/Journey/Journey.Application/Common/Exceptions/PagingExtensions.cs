using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Common.Exceptions;
public static class PagingExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
       this IQueryable<T> query, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);

        return new PagedList<T>(items, page, pageSize, total);
    }
}
