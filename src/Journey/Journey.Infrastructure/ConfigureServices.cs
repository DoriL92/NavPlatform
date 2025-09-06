using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Infrastructure.Identity;
using CleanArchitecture.Infrastructure.Messaging;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Persistence.Analytics;
using CleanArchitecture.Infrastructure.Persistence.Interceptors;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IMonthlyDistanceProjector, MonthlyDistanceProjector>();


        services.AddHttpContextAccessor();



        services.Configure<RabbitOptions>(configuration.GetSection("Rabbit"));

     


        services.AddScoped<PublishDomainEventsInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure(); 
                    })
                   .AddInterceptors(
                       sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
                    sp.GetRequiredService<PublishDomainEventsInterceptor>() 
                   );

           
        });

        services.AddSingleton<IConnection>(sp =>
        {
            var c = sp.GetRequiredService<IConfiguration>();
            var factory = new ConnectionFactory
            {
                HostName = c["Rabbit:Host"] ?? "rabbitmq",
                Port = int.TryParse(c["Rabbit:Port"], out var p) ? p : 5672,
                UserName = c["Rabbit:User"] ?? "guest",
                Password = c["Rabbit:Pass"] ?? "guest",
                DispatchConsumersAsync = true
            };
            return factory.CreateConnection("journey-api");
        });

        services.AddSingleton<IRewardsBus, RabbitRewardsBus>();
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddTransient<IDateTime, DateTimeService>();

        return services;
    }
}
