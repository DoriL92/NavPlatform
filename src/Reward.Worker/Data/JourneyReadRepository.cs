using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Reward.Worker.Data;
public sealed class JourneyReadRepository
{
    private readonly string _cs;
    public JourneyReadRepository(IConfiguration cfg) =>
        _cs = cfg.GetConnectionString("JourneyDb")!;

    public async Task<List<(int id, decimal km)>> GetUserDayJourneys(string ownerUserId, DateTimeOffset dayUtc, CancellationToken ct)
    {
        using var cn = new SqlConnection(_cs);
        await cn.OpenAsync(ct);

        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
SELECT Id, DistanceKm
FROM Journeys
WHERE OwnerUserId = @user
  AND CAST(SWITCHOFFSET([StartTime], '+00:00') AS date) = @day
  AND IsDeleted = 0
ORDER BY StartTime ASC, Id ASC;";

        cmd.Parameters.Add(new SqlParameter("@user", SqlDbType.NVarChar, 128) { Value = ownerUserId });
        cmd.Parameters.Add(new SqlParameter("@day", SqlDbType.Date) { Value = dayUtc });

        var list = new List<(int, decimal)>();
        using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add((r.GetInt32(0), r.GetDecimal(1)));
        return list;
    }

    public async Task<bool> TryInsertDailyGoal(string ownerUserId, DateTimeOffset dayUtc, int journeyId, DateTimeOffset nowUtc, CancellationToken ct)
    {
        using var cn = new SqlConnection(_cs);
        await cn.OpenAsync(ct);

        using var cmd = cn.CreateCommand();
        cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM DailyGoals WHERE OwnerUserId=@u AND DayUtc=@d)
BEGIN
    INSERT DailyGoals(OwnerUserId, DayUtc, AchievedJourneyId, AchievedAtUtc)
    VALUES (@u, @d, @j, @now);
    SELECT 1;
END
ELSE SELECT 0;";

        cmd.Parameters.Add(new SqlParameter("@u", SqlDbType.NVarChar, 128) { Value = ownerUserId });
        cmd.Parameters.Add(new SqlParameter("@d", SqlDbType.Date) { Value = dayUtc });
        cmd.Parameters.Add(new SqlParameter("@j", SqlDbType.Int) { Value = journeyId });
        cmd.Parameters.Add(new SqlParameter("@now", SqlDbType.DateTimeOffset) { Value = nowUtc });

        var res = (int)await cmd.ExecuteScalarAsync(ct);
        return res == 1;
    }

    public async Task<bool> AlreadyAchieved(string ownerUserId, DateTimeOffset dayUtc, CancellationToken ct)
    {
        using var cn = new SqlConnection(_cs);
        await cn.OpenAsync(ct);
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM DailyGoals WHERE OwnerUserId=@u AND DayUtc=@d";
        cmd.Parameters.Add(new SqlParameter("@u", SqlDbType.NVarChar, 128) { Value = ownerUserId });
        cmd.Parameters.Add(new SqlParameter("@d", SqlDbType.Date) { Value = dayUtc });
        var res = await cmd.ExecuteScalarAsync(ct);
        return res is not null;
    }
}