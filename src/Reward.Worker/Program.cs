using System.Reflection;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Rewards;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Messaging;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;



var builder = Host.CreateApplicationBuilder(args);
var cfg = builder.Configuration;
var services = builder.Services;

services.AddLogging(o => { o.ClearProviders(); o.AddConsole(); });

services.AddApplicationServices();
services.AddInfrastructureServices(cfg);


services.Configure<RabbitOptions>(cfg.GetSection("Rabbit"));
builder.Services.AddSingleton<IConnection>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();

    var factory = new ConnectionFactory
    {
        // fallbacks if env vars are missing
        HostName = cfg.GetValue<string>("Rabbit:Host", "rabbitmq"),
        Port = cfg.GetValue<int>("Rabbit:Port", 5672),
        UserName = cfg.GetValue<string>("Rabbit:User", "guest"),
        Password = cfg.GetValue<string>("Rabbit:Pass", "guest"),

        DispatchConsumersAsync = true,
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
        RequestedConnectionTimeout = TimeSpan.FromSeconds(10)
    };

    return factory.CreateConnection("reward-worker");
});

services.AddScoped<IDailyRewardService, DailyRewardService>();
services.AddScoped<IDailyGoalCalculator, DailyGoalCalculator>();
services.AddScoped<IUserDirectory, UserDirectory>();


services.AddHostedService<RewardsConsumer>();


services.AddSingleton<IModel>(sp =>
{
    var ch = sp.GetRequiredService<IConnection>().CreateModel();
    var opt = sp.GetRequiredService<IOptions<RabbitOptions>>().Value;

    // declare exchange/queue/bindings as needed
    var exchange = opt.Exchange ?? "nav.events";
    var queue = opt.Queue ?? "daily-goal-achieved";

    ch.ExchangeDeclare(exchange, type: "topic", durable: true);
    ch.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

    // example bindings
    ch.QueueBind(queue, exchange, routingKey: "journey.dailygoal.achieved");
    ch.QueueBind(queue, exchange, routingKey: "journey.updated");

    return ch;
});


var app = builder.Build();


await app.RunAsync();
