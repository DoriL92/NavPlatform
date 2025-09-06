using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class UnshareByEmailHandler(
    IApplicationDbContext db,
    ICurrentUser me,
    IUserDirectory users,
    IDateTime clock
) : IRequestHandler<UnshareByEmailCommand>
{
    public async Task Handle(UnshareByEmailCommand r, CancellationToken ct)
    {
        var j = await db.EntitySet<JourneyEntity>().FindAsync(new object[] { r.JourneyId }, ct)
                ?? throw new NotFoundException(nameof(JourneyEntity), r.JourneyId);

        if (j.OwnerUserId != me.UserId)
            throw new ForbiddenAccessException();

        var map = await users.ResolveUserIdsByEmailAsync(r.Emails, ct);
        var targetIds = map.Values.ToArray();

        var shares = await db.EntitySet<JourneyShare>()
            .Where(s => s.JourneyId == r.JourneyId
                     && targetIds.Contains(s.TargetUserId)
                     && s.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var s in shares)
        {
            s.RevokedAt = clock.Now;
            db.EntitySet<JourneyShareAudit>().Add(new JourneyShareAudit
            {
                JourneyId = r.JourneyId,
                ActorUserId = me.UserId!,
                Action = "unshare",
                Details = JsonSerializer.Serialize(new { targetId = s.TargetUserId }),
                At = clock.Now
            });
        }

        await db.SaveChangesAsync(ct);
    }
}