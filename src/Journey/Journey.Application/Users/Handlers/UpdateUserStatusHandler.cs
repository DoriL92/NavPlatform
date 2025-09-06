using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Users.Commands;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Users.Handlers;

public sealed class UpdateUserStatusHandler : IRequestHandler<UpdateUserStatusCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _dateTime;

    public UpdateUserStatusHandler(
        IApplicationDbContext db, 
        ICurrentUser currentUser, 
        IDateTime dateTime)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserStatus>(request.Status, true, out var newStatus))
        {
            throw new ValidationException($"Invalid status '{request.Status}'. Valid values are: Active, Suspended, Deactivated");
        }

        var user = await _db.EntitySet<User>()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User with ID '{request.UserId}' was not found.");
        }

        var adminUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Admin user ID is required");
        var previousStatus = user.Status;
        
        user.UpdateStatus(newStatus, adminUserId, _dateTime.Now);

        var audit = new UserStatusAudit
        {
            UserId = user.Id!,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            AdminUserId = adminUserId,
            ChangedAt = _dateTime.Now.DateTime
        };

        _db.EntitySet<UserStatusAudit>().Add(audit);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
