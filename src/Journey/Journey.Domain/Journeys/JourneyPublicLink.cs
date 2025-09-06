using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Journeys;
public class JourneyPublicLink
{
    public int Id { get; set; }
    public int JourneyId { get; set; }
    public JourneyEntity Journey { get; set; }
    public string Token { get; set; } = default!;       
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}