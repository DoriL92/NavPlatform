using CleanArchitecture.Application.Common.Models;
using MediatR;

namespace CleanArchitecture.Application.Statistics.Queries;

public record GetMonthlyDistanceStatisticsQuery(
    int Page = 1,
    int PageSize = 20,
    string? OrderBy = null
) : IRequest<PagedList<MonthlyDistanceDto>>;

public class MonthlyDistanceDto
{
    public string UserId { get; set; } = default!;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalDistanceKm { get; set; }
}

