using System.Reflection;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Journeys.Services;
using CleanArchitecture.Application.Rewards;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddFluentValidationAutoValidation();
        services.AddScoped<IDailyRewardService, DailyRewardService>();
        services.AddScoped<IDailyGoalCalculator, DailyGoalCalculator>();
        services.AddScoped<IDailyGoalCheckService, DailyGoalCheckService>();


        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<NotFoundException>();
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());         
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        });

        return services;
    }
}
