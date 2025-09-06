using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Common.Models;

public sealed class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    public PagedList(IReadOnlyList<T> items, int page, int pageSize, int total)
        => (Items, Page, PageSize, TotalCount) = (items, page, pageSize, total);
}

public static class PagingExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> q, int page, int pageSize, CancellationToken ct)
    {
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedList<T>(items, page, pageSize, total);
    }
}
