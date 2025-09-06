using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys;

namespace CleanArchitecture.Application.Rewards;
public sealed class DailyGoalCalculator : IDailyGoalCalculator
{
    public JourneyEntity? FindBadgeJourney(IEnumerable<JourneyEntity> journeysSameDay, out decimal totalKm)
    {
        decimal sum = 0m;
        JourneyEntity? trigger = null;

        foreach (var j in journeysSameDay.OrderBy(x => x.StartTime).ThenBy(x => x.Id))
        {
            sum += j.DistanceKm.Value;
            if (sum >= 20.00m) { trigger = j; break; }   
        }

        totalKm = sum;
        return trigger;
    }
}