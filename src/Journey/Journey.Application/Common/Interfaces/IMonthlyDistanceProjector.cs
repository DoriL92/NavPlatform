using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys;

namespace CleanArchitecture.Application.Common.Interfaces;
public interface IMonthlyDistanceProjector
{
    Task ApplyCreatedAsync(JourneyEntity j, CancellationToken ct);
    Task ApplyUpdatedAsync(JourneyEntity before, JourneyEntity after, CancellationToken ct);
    Task ApplyDeletedAsync(JourneyEntity j, CancellationToken ct);
}