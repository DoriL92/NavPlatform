using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Domain.Journeys;
using MediatR;

namespace CleanArchitecture.Application.Journeys.Commands;
public record UpdateJourneyCommand(
    int Id,
    string StartLocation, DateTimeOffset StartTime,
    string ArrivalLocation, DateTimeOffset ArrivalTime,
    string TransportType, decimal DistanceKm, bool IsDailyGoalAchieved
) : IRequest<Unit>;


