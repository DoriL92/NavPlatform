using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Journeys.Commands;
using CleanArchitecture.Application.Journeys.Dto;
using FluentValidation;

namespace CleanArchitecture.Application.Journeys.Validators;
public class CreateJourneyCommandValidator : AbstractValidator<CreateJourneyCommand>
{
    public CreateJourneyCommandValidator()
    {
        RuleFor(x => x.StartLocation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ArrivalLocation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.ArrivalTime).NotEmpty().GreaterThan(x => x.StartTime)
            .WithMessage("ArrivalTime must be after StartTime.");
        RuleFor(x => x.DistanceKm).InclusiveBetween(0, 999.99m);
        RuleFor(x => x.TransportType).NotEmpty();
    }

    internal IValidator<UpdateJourneyCommand> IncludeRules(Func<object, CreateJourneyCommand> value)
    {
        throw new NotImplementedException();
    }
}
