using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class RevokePublicLinkHandler(
    IApplicationDbContext db,
    ICurrentUser me,
    IDateTime clock
) : IRequestHandler<RevokePublicLinkCommand>
{
    public async Task Handle(RevokePublicLinkCommand r, CancellationToken ct)
    {
        var journey = await db.EntitySet<JourneyEntity>().FindAsync(new object[] { r.JourneyId }, ct)
                     ?? throw new NotFoundException(nameof(JourneyEntity), r.JourneyId);

        if (journey.OwnerUserId != me.UserId)
            throw new ForbiddenAccessException();

        var active = await db.EntitySet<JourneyPublicLink>()
            .FirstOrDefaultAsync(x => x.JourneyId == r.JourneyId && x.RevokedAt == null, ct);

        if (active is null) return; 

        active.RevokedAt = clock.Now;

        db.EntitySet<JourneyShareAudit>().Add(new JourneyShareAudit
        {
            JourneyId = r.JourneyId,
            ActorUserId = me.UserId!,
            Action = "public-link-revoked",
            Details = JsonSerializer.Serialize(new { token = active.Token }),
            At = clock.Now
        });

        await db.SaveChangesAsync(ct);
    }
}