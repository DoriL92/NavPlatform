using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Abstractions.Messaging;
public interface IRewardsBus
{
    Task PublishRecalcAsync(string userId, DateTimeOffset dayUtc, CancellationToken ct);
    Task PublishAsync(string routingKey, object payload, CancellationToken ct = default);
    Task SubscribeAsync<T>(string topic, Func<T, CancellationToken, Task> handler, CancellationToken stoppingToken);
}


