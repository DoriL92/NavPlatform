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
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class CreatePublicLinkHandler : IRequestHandler<CreatePublicLinkCommand, string>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IDateTime _clock;

    public CreatePublicLinkHandler(
        IApplicationDbContext db,
        ICurrentUser me,
        IDateTime clock)
    {
        _db = db;
        _user = me;
        _clock = clock;
    }

    public async Task<string> Handle(CreatePublicLinkCommand r, CancellationToken ct)
    {
        var journey = await _db.EntitySet<JourneyEntity>().FindAsync(new object[] { r.JourneyId }, ct)
                     ?? throw new NotFoundException(nameof(JourneyEntity), r.JourneyId);

        if (journey.OwnerUserId != _user.UserId)
            throw new ForbiddenAccessException();

        var active = await _db.EntitySet<JourneyPublicLink>()
            .FirstOrDefaultAsync(x => x.JourneyId == r.JourneyId && x.RevokedAt == null, ct);

        if (active is null)
        {
            var token = CreateUrlSafeToken(24);
            active = new JourneyPublicLink
            {
                JourneyId = r.JourneyId,
                Token = token,
                CreatedAt = _clock.Now
            };

            _db.EntitySet<JourneyPublicLink>().Add(active);

            _db.EntitySet<JourneyShareAudit>().Add(new JourneyShareAudit
            {
                JourneyId = r.JourneyId,
                ActorUserId = _user.UserId!,
                Action = "public-link-created",
                Details = JsonSerializer.Serialize(new { token }),
                At = _clock.Now
            });

            await _db.SaveChangesAsync(ct);
        }

        return active.Token;
    }

    private static string CreateUrlSafeToken(int bytes)
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes))
             .Replace("+", "-").Replace("/", "_").TrimEnd('=');
}
