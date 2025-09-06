using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Journeys;
public class JourneyFavorite
{
    public int Id { get; private set; }
    public int JourneyId { get; private set; }
    public string UserId { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    public JourneyEntity? Journey { get; private set; }

    private JourneyFavorite() { }
    public static JourneyFavorite Create(int journeyId, string userId, DateTimeOffset now)
        => new() { JourneyId = journeyId, UserId = userId, CreatedAt = now };
}

