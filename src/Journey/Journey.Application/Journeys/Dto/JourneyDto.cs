using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.Common.Mappings;
using CleanArchitecture.Domain.Journeys;

namespace CleanArchitecture.Application.Journeys.Dto;

public class JourneyDto
{
    public int Id { get; set; }
    public string OwnerUserId { get;  set; } = default!;
    public string StartLocation { get; set; } = default!;
    public DateTimeOffset StartTime { get; set; }
    public string ArrivalLocation { get; set; } = default!;
    public DateTimeOffset ArrivalTime { get; set; }
    public string TransportType { get; set; } = default!;
    public decimal DistanceKm { get; set; }
    public bool IsDailyGoalAchieved { get; set; }

    public bool IsOwnedByMe { get; set; }
    public bool IsShared { get; set; }
    public bool IsFavorite { get; set; }=false;
}

public class JourneyMapping : Profile
{
    public JourneyMapping()
    {
        CreateMap<JourneyEntity, JourneyDto>()
            .ForMember(d => d.TransportType, o => o.MapFrom(s => s.TransportType.ToString()))
            .ForMember(d => d.DistanceKm, o => o.MapFrom(s => s.DistanceKm.Value))
            .ForMember(d => d.IsOwnedByMe, o => o.Ignore())  
            .ForMember(d => d.IsShared, o => o.Ignore()); 
    }
}
