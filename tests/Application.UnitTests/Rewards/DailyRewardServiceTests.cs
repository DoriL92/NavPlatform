using System.Collections.Generic;
using Application.UnitTests.Support;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Rewards;
using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Persistence.Interceptors;
using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class SqliteDbScope : IDisposable
{
    public ApplicationDbContext Db { get; }
    private readonly SqliteConnection _conn;

    public SqliteDbScope(string userId)
    {
        _conn = new SqliteConnection("Filename=:memory:");
        _conn.Open(); // keep the in-memory DB alive

        var services = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .AddSingleton<ICurrentUser>(new TestCurrentUser(userId))
            .AddSingleton<IMediator, NoopMediator>()
            .AddScoped<AuditableEntitySaveChangesInterceptor>(sp =>
                new AuditableEntitySaveChangesInterceptor(sp.GetRequiredService<ICurrentUser>(), null));

        var provider = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_conn)
            .UseInternalServiceProvider(provider)
            .EnableSensitiveDataLogging()
            .Options;

        Db = new ApplicationDbContext(
            options,
            provider.GetRequiredService<IMediator>(),
            provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
            provider.GetRequiredService<ICurrentUser>());


        var sql = Db.Database.GenerateCreateScript();
        Console.WriteLine(sql);
            
            Db.Database.EnsureCreated();


    }

    public void Dispose()
    {
        Db.Dispose();
        _conn.Dispose();
    }
}

public sealed class DailyRewardServiceTests
{
    private const string Uid = "auth0|test-user";

    [Fact]
    public async Task Total_19_99km__no_award()
    {
        using var db = CreateDbInMemory(Uid);
        var clock = new TestClock(new DateTimeOffset(2025, 9, 1, 0, 0, 0, TimeSpan.Zero));
        var calc = new DailyGoalCalculator(); // your implementation of IDailyGoalCalculator

        await SeedJourneys(db, 10.50m, 9.49m); // 19.99 total

        var svc = new DailyRewardService(db, calc, clock);
        var day = DateOnly.FromDateTime(clock.Now.UtcDateTime);

        var (awarded, total) = await svc.CheckAndAwardAsync(Uid, day);

        awarded.Should().BeFalse();
        total.Should().Be(19.99m);
        (await db.EntitySet<JourneyEntity>().AnyAsync(j => j.IsDailyGoalAchieved)).Should().BeFalse();
    }

    [Fact]
    public async Task Total_20_00km__no_award()
    {
        using var db = CreateDbInMemory(Uid);
        var clock = new TestClock(new DateTimeOffset(2025, 9, 1, 0, 0, 0, TimeSpan.Zero));
        var calc = new DailyGoalCalculator();

        await SeedJourneys(db, 10.00m, 10.00m); // 20.00 total

        var svc = new DailyRewardService(db, calc, clock);
        var day = DateOnly.FromDateTime(clock.Now.UtcDateTime);

        var (awarded, total) = await svc.CheckAndAwardAsync(Uid, day);

        awarded.Should().BeFalse();            // rule is "exceeds" 20.00
        total.Should().Be(20.00m);
        (await db.EntitySet<JourneyEntity>().AnyAsync(j => j.IsDailyGoalAchieved)).Should().BeFalse();
    }

    [Fact]
    public async Task Total_20_01km__awards_once_on_triggering_journey()
    {
        using var db = CreateDbInMemory(Uid);
        var clock = new TestClock(new DateTimeOffset(2025, 9, 1, 0, 0, 0, TimeSpan.Zero));
        var calc = new DailyGoalCalculator();

        await SeedJourneys(db, 10.00m, 10.01m); // 20.01 total -> award on the second journey

        var svc = new DailyRewardService(db, calc, clock);
        var day = DateOnly.FromDateTime(clock.Now.UtcDateTime);

        var (awarded, total) = await svc.CheckAndAwardAsync(Uid, day);

        awarded.Should().BeTrue();
        total.Should().Be(20.01m);

        var journeys = await db.EntitySet<JourneyEntity>().OrderBy(j => j.Id).ToListAsync();
        journeys.Count(j => j.IsDailyGoalAchieved).Should().Be(1);
        journeys.Last().IsDailyGoalAchieved.Should().BeTrue(); // badge set on the triggering journey
    }

    // ---------- helpers ----------

    private static async Task SeedJourneys(IApplicationDbContext db, params decimal[] kms)
    {
        var baseStart = new DateTimeOffset(2025, 9, 1, 8, 0, 0, TimeSpan.Zero);
        for (int i = 0; i < kms.Length; i++)
        {
            var j = JourneyEntity.Create(
                ownerUserId: Uid,
                email:"Test",
                from: "A",
                start: baseStart.AddHours(i),
                to: "B",
                arrive: baseStart.AddHours(i + 1),
                t: TransportType.Bike,
                km: DistanceKm.From(kms[i]),
                nowUtc: baseStart,
                isDailyGoalAchieved: false);

            db.EntitySet<JourneyEntity>().Add(j);
        }
        await db.SaveChangesAsync(CancellationToken.None);
    }

    private static ApplicationDbContext CreateDbInMemory(string userId)
    {
        var services = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddSingleton<ICurrentUser>(new TestCurrentUser(userId))
            .AddSingleton<IMediator, NoopMediator>()
            .AddScoped<AuditableEntitySaveChangesInterceptor>(sp =>
                new AuditableEntitySaveChangesInterceptor(sp.GetRequiredService<ICurrentUser>(), /* ILogger? */ null));

        var provider = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseInternalServiceProvider(provider)   
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(
            options,
            provider.GetRequiredService<IMediator>(),
            provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
            provider.GetRequiredService<ICurrentUser>());
    }
}