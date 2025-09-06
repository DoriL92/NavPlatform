using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Domain.Journeys;
using MediatR;

namespace CleanArchitecture.Application.Journeys.Commands;
public record AdminJourneyFilterQuery(
    string? UserId,
    TransportType TransportType,
    DateTimeOffset? StartDateFrom,
    DateTimeOffset? StartDateTo,
    DateTimeOffset? ArrivalDateFrom,
    DateTimeOffset? ArrivalDateTo,
    decimal? MinDistance,
    decimal? MaxDistance,
    int Page = 1,
    int PageSize = 20,
    string? OrderBy = null,   
    string? Direction = null  
) : IRequest<(IReadOnlyList<JourneyDto> Items, int Total)>;
