using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Handlers;
public sealed class FavouriteJourneyHandler : IRequestHandler<FavouriteJourneyCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IDateTime _clock;

    public FavouriteJourneyHandler(IApplicationDbContext db, ICurrentUser user, IDateTime clock)
        => (_db, _user, _clock) = (db, user, clock);

    public async Task Handle(FavouriteJourneyCommand r, CancellationToken ct)
    {
        var uid = _user.UserId ?? throw new UnauthorizedAccessException();

        var exists = await _db.EntitySet<JourneyFavorite>()
            .AnyAsync(x => x.JourneyId == r.JourneyId && x.UserId == uid, ct);
        if (exists) return; 

        _db.EntitySet<JourneyFavorite>()
           .Add(JourneyFavorite.Create(r.JourneyId, uid, _clock.Now));
        await _db.SaveChangesAsync(ct);
    }
}