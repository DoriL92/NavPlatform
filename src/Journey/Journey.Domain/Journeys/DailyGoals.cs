using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Journeys;
public sealed class DailyGoal
{
    public string OwnerUserId { get; set; } = default!;
    public DateOnly DayUtc { get; set; }

    public int AchievedJourneyId { get; set; }
    public DateTime AchievedAtUtc { get; set; }
}