using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class DeleteJourneyHandler : IRequestHandler<DeleteJourneyCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IDateTime _clock;

    public DeleteJourneyHandler(IApplicationDbContext db, ICurrentUser user, IDateTime clock)
        => (_db, _user, _clock) = (db, user, clock);

    public async Task<Unit> Handle(DeleteJourneyCommand r, CancellationToken ct)
    {
        var j = await _db.EntitySet<JourneyEntity>().FirstOrDefaultAsync(x => x.Id == r.Id, ct)
                ?? throw new NotFoundException(nameof(JourneyEntity), r.Id);

        if (j.OwnerUserId != _user.UserId)
            throw new ForbiddenAccessException();

        j.Delete(_clock.Now);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
