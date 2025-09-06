using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Journeys;

namespace CleanArchitecture.Infrastructure.Persistence.Analytics;
public sealed class MonthlyDistanceProjector : IMonthlyDistanceProjector
{
    private readonly IApplicationDbContext _db;
    public MonthlyDistanceProjector(IApplicationDbContext db) => _db = db;

    static (int y, int m) Bucket(DateTimeOffset dt)
      => (dt.UtcDateTime.Year, dt.UtcDateTime.Month);

    public async Task ApplyCreatedAsync(JourneyEntity j, CancellationToken ct)
    {
        var (y, m) = Bucket(j.StartTime);
        var set = _db.EntitySet<MonthlyDistance>();
        var row = await set.FindAsync([j.OwnerUserId, y, m], ct)
              ?? new MonthlyDistance { UserId = j.OwnerUserId, Year = y, Month = m };
        row.TotalDistanceKm += j.DistanceKm.Value;
        set.Update(row);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ApplyUpdatedAsync(JourneyEntity before, JourneyEntity after, CancellationToken ct)
    {
        var (y1, m1) = Bucket(before.StartTime);
        var (y2, m2) = Bucket(after.StartTime);
        var set = _db.EntitySet<MonthlyDistance>();

        if (y1 == y2 && m1 == m2)
        {
            var row = await set.FindAsync([after.OwnerUserId, y2, m2], ct)
                      ?? new MonthlyDistance { UserId = after.OwnerUserId, Year = y2, Month = m2 };
            row.TotalDistanceKm += (after.DistanceKm.Value - before.DistanceKm.Value);
            set.Update(row);
        }
        else
        {
            var r1 = await set.FindAsync([before.OwnerUserId, y1, m1], ct);
            if (r1 != null) { r1.TotalDistanceKm -= before.DistanceKm.Value; set.Update(r1); }

            var r2 = await set.FindAsync([after.OwnerUserId, y2, m2], ct)
                     ?? new MonthlyDistance { UserId = after.OwnerUserId, Year = y2, Month = m2 };
            r2.TotalDistanceKm += after.DistanceKm.Value;
            set.Update(r2);
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ApplyDeletedAsync(JourneyEntity j, CancellationToken ct)
    {
        var (y, m) = Bucket(j.StartTime);
        var row = await _db.EntitySet<MonthlyDistance>().FindAsync([j.OwnerUserId, y, m], ct);
        if (row != null)
        {
            row.TotalDistanceKm -= j.DistanceKm.Value;
            await _db.SaveChangesAsync(ct);
        }
    }
}