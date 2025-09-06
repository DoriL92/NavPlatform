using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Commands;
public record MarkDailyGoalAchievedCommand(int JourneyId, DateTimeOffset NowUtc, decimal km) : IRequest;
public sealed class MarkDailyGoalAchievedHandler : IRequestHandler<MarkDailyGoalAchievedCommand>
{
    private readonly IApplicationDbContext _db;
    public MarkDailyGoalAchievedHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(MarkDailyGoalAchievedCommand r, CancellationToken ct)
    {
        var j = await _db.EntitySet<JourneyEntity>()
                         .FirstOrDefaultAsync(x => x.Id == r.JourneyId, ct)
                ?? throw new NotFoundException(nameof(JourneyEntity), r.JourneyId);

        if (!j.IsDailyGoalAchieved)
        {
            j.MarkDailyGoalAchieved(r.NowUtc,r.km);
            await _db.SaveChangesAsync(ct);
        }
    }
}
