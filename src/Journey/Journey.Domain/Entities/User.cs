using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Domain.Events;

namespace CleanArchitecture.Domain.Entities;
public class User: BaseAuditableEntity
{

    public string Id { get; private set; } = default!;
    public string? Email { get; set; }
    public string? Name { get; set; }

    private User() { }

    public User(string id, string? email = null, string? name = null)
    {
        Id = id;
        Email = email;
        Name = name;
        CreatedAt = DateTime.UtcNow;
        LastSeenAt = DateTime.UtcNow;
        Status = UserStatus.Active;
    }
    public string? PictureUrl { get; set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public ICollection<JourneyEntity> OwnedJourneys { get; set; } = new List<JourneyEntity>();
    public ICollection<JourneyShare> SharesReceived { get; set; } = new List<JourneyShare>();
    public ICollection<UserStatusAudit> StatusAudits { get; set; } = new List<UserStatusAudit>();

    public void UpdateStatus(UserStatus newStatus, string adminUserId, DateTimeOffset utcNow)
    {
        if (Status == newStatus) return;

        var previousStatus = Status;
        Status = newStatus;
        
        AddDomainEvent(new UserStatusChanged(Id!, previousStatus, newStatus, adminUserId, utcNow));
    }
}
