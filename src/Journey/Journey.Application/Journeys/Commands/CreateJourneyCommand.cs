using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Domain.Journeys;
using MediatR;

namespace CleanArchitecture.Application.Journeys.Commands;
public record CreateJourneyCommand(
    string StartLocation, DateTimeOffset StartTime,
    string ArrivalLocation, DateTimeOffset ArrivalTime,
    string TransportType, decimal DistanceKm, bool IsDailyGoalAchieved
) : IRequest<int>;

