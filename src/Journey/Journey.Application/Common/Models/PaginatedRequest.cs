namespace CleanArchitecture.Application.Common.Models;
public class PaginatedRequest
{
    private const int maxPageSize = 10000;
    private int _pageSize = 15;
    private int _pageIndex = 1;

    public int PageNumber
    {
        get => _pageIndex;
        set => _pageIndex = value <= 0 ? _pageIndex : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > maxPageSize ? maxPageSize : value <= 0 ? _pageSize : value;
    }
    public string? OrderBy { get; set; }
    public OrderEnum Order { get; set; } = OrderEnum.Asc;
}


public interface ISortOrder
{
    public string? OrderBy { get; set; }
    public OrderEnum Order { get; set; }
}

public enum OrderEnum
{
    Asc,
    Desc
}


