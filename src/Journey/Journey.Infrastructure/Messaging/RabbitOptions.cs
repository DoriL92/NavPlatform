using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Messaging;
public sealed class RabbitOptions
{
    public string Host { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "guest";
    public string Pass { get; set; } = "guest";
    public string Exchange { get; set; } = "nav.events";
    public string Queue { get; set; } = "rewards.dailygoal";
    public string RoutingKey { get; set; } = "journey.*";
}