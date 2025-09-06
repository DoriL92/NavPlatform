using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Abstractions.Messaging;
public interface IEmailQueue
{
    Task EnqueueJourneyUpdateAsync(string userId, int journeyId, string kind, CancellationToken ct);
    Task EnqueueJourneySharedAsync(string recipientUserId, int journeyId, string actorUserId, CancellationToken ct);
    Task EnqueueJourneyUnsharedAsync(string recipientUserId, int journeyId, string actorUserId, CancellationToken ct);
    Task EnqueueDailyGoalAsync(string userId, DateOnly date, double totalKm, CancellationToken ct);
}
