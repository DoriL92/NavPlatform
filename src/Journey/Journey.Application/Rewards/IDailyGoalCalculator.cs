using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys;

namespace CleanArchitecture.Application.Rewards;
public interface IDailyGoalCalculator
{
    JourneyEntity? FindBadgeJourney(IEnumerable<JourneyEntity> journeysSameDay, out decimal totalKm);
}
