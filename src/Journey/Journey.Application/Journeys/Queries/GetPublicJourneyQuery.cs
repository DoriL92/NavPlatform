using CleanArchitecture.Application.Journeys.Dto;
using MediatR;

namespace CleanArchitecture.Application.Journeys.Queries;

public sealed record GetPublicJourneyQuery(string Token) : IRequest<JourneyDto>;
