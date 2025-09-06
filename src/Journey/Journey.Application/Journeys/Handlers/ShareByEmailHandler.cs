using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Options;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Events;
using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Domain.Journeys.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class ShareByEmailHandler(
    IApplicationDbContext db,
    ICurrentUser me,
    IUserDirectory users,
    IDateTime clock
) : IRequestHandler<ShareByEmailCommand, ShareByEmailResult>
{
    public async Task<ShareByEmailResult> Handle(ShareByEmailCommand r, CancellationToken ct)
    {
        var journey = await db.EntitySet<JourneyEntity>()
            .FindAsync(new object[] { r.JourneyId }, ct)
            ?? throw new NotFoundException(nameof(JourneyEntity), r.JourneyId);

        if (journey.OwnerUserId != me.UserId)
            throw new ForbiddenAccessException();

        var resolved = await users.ResolveUserIdsByEmailAsync(r.Emails, ct);
        var now = clock.Now;
        var added = 0;

        foreach (var kv in resolved)
        {
            var email = kv.Key;
            var targetId = kv.Value;

            var exists = await db.EntitySet<JourneyShare>()
                .AnyAsync(s => s.JourneyId == r.JourneyId && s.TargetUserId == targetId && s.RevokedAt == null, ct);

            if (exists) continue;

            db.EntitySet<JourneyShare>().Add(new JourneyShare
            {
                JourneyId = r.JourneyId,
                TargetUserId = kv.Value,
                GrantedByUserId = me.UserId!,
                GrantedAt = now
            });

            db.EntitySet<JourneyShareAudit>().Add(new JourneyShareAudit
            {
                JourneyId = r.JourneyId,
                ActorUserId = me.UserId!,
                Action = "share",
                Details = JsonSerializer.Serialize(new { email, targetId }),
                At = now
            });

            added++;
        }

        await db.SaveChangesAsync(ct);

        // Count total active shares after insert
        var shareCount = await db.EntitySet<JourneyShare>()
            .CountAsync(s => s.JourneyId == r.JourneyId && s.RevokedAt == null, ct);

        return new ShareByEmailResult(true, shareCount);
    }
}