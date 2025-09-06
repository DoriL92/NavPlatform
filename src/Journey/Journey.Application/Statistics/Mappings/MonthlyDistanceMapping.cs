using AutoMapper;
using CleanArchitecture.Application.Statistics.Queries;
using CleanArchitecture.Domain.Journeys;

namespace CleanArchitecture.Application.Statistics.Mappings;

public class MonthlyDistanceMapping : Profile
{
    public MonthlyDistanceMapping()
    {
        CreateMap<MonthlyDistance, MonthlyDistanceDto>();
    }
}

