using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.UnitTests.Support;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Persistence.Interceptors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests;
public static class TestServices
{
    public static ServiceProvider Build(string dbName, string userId)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        // REQUIRED by ApplicationDbContext
        services.AddSingleton<IMediator, NoopMediator>();                  
        services.AddSingleton<ICurrentUser>(new TestCurrentUser(userId));
        services.AddSingleton<IDateTime>(new TestClock());
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        // DbContext (InMemory)
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseInMemoryDatabase(dbName).EnableSensitiveDataLogging());

        // Expose via app interface
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services.BuildServiceProvider();
    }
}
