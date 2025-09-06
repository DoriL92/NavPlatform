using CleanArchitecture.Domain.Enums;
using MediatR;

namespace CleanArchitecture.Domain.Events;

public sealed record UserStatusChanged(
    string UserId, 
    UserStatus PreviousStatus, 
    UserStatus NewStatus, 
    string AdminUserId, 
    DateTimeOffset OccurredOn
) : IDomainEvent;
