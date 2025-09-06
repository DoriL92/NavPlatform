using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Journeys;

public class JourneyShareAudit
{
    public long Id { get; set; }
    public int JourneyId { get; set; }
    public string ActorUserId { get; set; } = default!;
    public string Action { get; set; } = default!;        
    public string? Details { get; set; }                 
    public DateTimeOffset At { get; set; }
}