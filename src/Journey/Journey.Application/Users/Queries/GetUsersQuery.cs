using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Domain.Enums;
using MediatR;

namespace CleanArchitecture.Application.Users.Queries;

public record GetUsersQuery(
    string? Email = null,
    string? Name = null,
    UserStatus? Status = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    int Page = 1,
    int PageSize = 20,
    string? OrderBy = null,
    string? Direction = null
) : IRequest<PagedList<UserDto>>;

public class UserDto
{
    public string Id { get; set; } = default!;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? PictureUrl { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastSeenAt { get; set; }
}

