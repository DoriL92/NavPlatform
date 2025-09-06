using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Journeys.Events;


namespace CleanArchitecture.Domain.Journeys;
public class JourneyEntity : BaseAuditableEntity
{

    public int Id { get; private set; }
    public string OwnerUserId { get; private set; } = default!;
    public string? OwnerEmail { get; private set; }
    public string StartLocation { get; private set; } = default!;
    public DateTimeOffset StartTime { get; private set; }
    public string ArrivalLocation { get; private set; } = default!;
    public DateTimeOffset ArrivalTime { get; private set; }
    public TransportType TransportType { get; private set; }
    public DistanceKm DistanceKm { get; private set; } = DistanceKm.From(0);
    public bool IsDailyGoalAchieved { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    //public User? Owner { get; private set; }

    public ICollection<JourneyShare> Shares { get; private set; } = new List<JourneyShare>();
    public JourneyPublicLink? PublicLink { get; private set; }


    public static JourneyEntity Create(
        string ownerUserId, string? email, string from, DateTimeOffset start,
        string to, DateTimeOffset arrive, TransportType t, DistanceKm km, DateTimeOffset nowUtc, bool isDailyGoalAchieved)
    {
        var j = new JourneyEntity
        {
            OwnerUserId   = ownerUserId,
            OwnerEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant(),
            StartLocation = from,
            StartTime     = start,
            ArrivalLocation = to,
            ArrivalTime   = arrive,
            TransportType = t,
            DistanceKm    = km,
            CreatedAt     = nowUtc,
            IsDailyGoalAchieved= isDailyGoalAchieved,

        };
        j.AddDomainEvent(new JourneyCreated(j.Id,j.OwnerUserId, nowUtc));
        return j;
    }

    public void Update(string from, DateTimeOffset start, string to, DateTimeOffset arrive,
                       TransportType t, DistanceKm km, DateTimeOffset nowUtc)
    {
        StartLocation = from; StartTime = start; ArrivalLocation = to; ArrivalTime = arrive;
        TransportType = t; DistanceKm = km; UpdatedAt = nowUtc;
        AddDomainEvent(new JourneyUpdated(Id, OwnerUserId, nowUtc));
    }

    public void Delete(DateTimeOffset nowUtc)
    {
        IsDeleted = true; UpdatedAt = nowUtc;
        AddDomainEvent(new JourneyDeleted(Id, OwnerUserId, nowUtc));
    }

    public void MarkDailyGoalAchieved(DateTimeOffset nowUtc, decimal totalKm)
    {
        if (IsDailyGoalAchieved) return;

        IsDailyGoalAchieved = true;
        UpdatedAt = nowUtc;
        AddDomainEvent(new DailyGoalAchieved(Id,
            OwnerUserId: OwnerUserId,
            Day: DateOnly.FromDateTime(StartTime.UtcDateTime),
            TotalKm: totalKm,
            OccurredOn: nowUtc));
    }
}