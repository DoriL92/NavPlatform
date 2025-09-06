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
public sealed class UnfavouriteJourneyHandler : IRequestHandler<UnfavouriteJourneyCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;

    public UnfavouriteJourneyHandler(IApplicationDbContext db, ICurrentUser user)
        => (_db, _user) = (db, user);

    public async Task Handle(UnfavouriteJourneyCommand r, CancellationToken ct)
    {
        var uid = _user.UserId ?? throw new UnauthorizedAccessException();

        var fav = await _db.EntitySet<JourneyFavorite>()
            .SingleOrDefaultAsync(x => x.JourneyId == r.JourneyId && x.UserId == uid, ct);

        if (fav is null) return; 

        _db.EntitySet<JourneyFavorite>().Remove(fav);
        await _db.SaveChangesAsync(ct);
    }
}