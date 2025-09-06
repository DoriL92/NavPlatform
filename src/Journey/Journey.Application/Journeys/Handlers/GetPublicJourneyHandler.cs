using AutoMapper;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Application.Journeys.Queries;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Handlers;

public sealed class GetPublicJourneyHandler : IRequestHandler<GetPublicJourneyQuery, JourneyDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetPublicJourneyHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<JourneyDto> Handle(GetPublicJourneyQuery r, CancellationToken ct)
    {
        var publicLink = await _db.EntitySet<JourneyPublicLink>()
            .Include(pl => pl.Journey)
            .FirstOrDefaultAsync(x => x.Token == r.Token && x.RevokedAt == null, ct);

        if (publicLink?.Journey == null)
            throw new NotFoundException(nameof(JourneyEntity), r.Token);

        return _mapper.Map<JourneyDto>(publicLink.Journey);
    }
}
