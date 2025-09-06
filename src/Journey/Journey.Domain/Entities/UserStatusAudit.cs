using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Domain.Entities;

public class UserStatusAudit
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public UserStatus PreviousStatus { get; set; }
    public UserStatus NewStatus { get; set; }
    public string AdminUserId { get; set; } = default!;
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }

    public User User { get; set; } = default!;
}

