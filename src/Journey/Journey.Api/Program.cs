using System;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Rewards;
using CleanArchitecture.Domain.Journeys;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Identity;
using CleanArchitecture.Infrastructure.Messaging;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Persistence.Interceptors;
using FluentValidation;
using Journey.Api.Middleware;
using Journey.Api.Realtime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

var rolesClaimType = "https://nav-platform.example.com/roles";

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        o.Audience = builder.Configuration["Auth0:Audience"];

        // Map your namespaced claim as the "role" claim
        o.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = rolesClaimType
        };

        o.Events = new JwtBearerEvents
        {
            // allow SignalR token via query
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            },

            // expand JSON array into multiple role claims if needed
            OnTokenValidated = ctx =>
            {
                var id = (ClaimsIdentity)ctx.Principal!.Identity!;
                foreach (var c in id.FindAll(rolesClaimType).ToList())
                {
                    var v = c.Value?.Trim();
                    if (!string.IsNullOrEmpty(v) && v.StartsWith("["))
                    {
                        try
                        {
                            var roles = JsonSerializer.Deserialize<string[]>(v) ?? [];
                            foreach (var r in roles)
                                id.AddClaim(new Claim(id.RoleClaimType, r));
                        }
                        catch { /* ignore parse errors */ }
                    }
                    else if (!string.IsNullOrEmpty(v))
                    {
                        id.AddClaim(new Claim(id.RoleClaimType, v));
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(o => o.AddPolicy("fe", p =>
    p.WithOrigins("http://localhost:4200", "https://localhost:4200")
     .AllowAnyHeader().AllowAnyMethod().AllowCredentials()
));

builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();

builder.Services.AddSingleton<IConnection>(sp =>
{
    var c = sp.GetRequiredService<IConfiguration>();
    var f = new ConnectionFactory
    {
        HostName = c["Rabbit:Host"] ?? "rabbitmq",
        Port = int.TryParse(c["Rabbit:Port"], out var p) ? p : 5672,
        UserName = c["Rabbit:User"] ?? "guest",
        Password = c["Rabbit:Pass"] ?? "guest",
        DispatchConsumersAsync = true
    };
    return f.CreateConnection("journey-api");
});
builder.Services.AddSingleton<IRewardsBus, RabbitRewardsBus>();



var appAssembly = typeof(AssemblyMarker).Assembly;
var apiAssembly = typeof(Program).Assembly;


builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(appAssembly);
    cfg.RegisterServicesFromAssembly(apiAssembly);
});
builder.Services.AddAutoMapper(appAssembly);
builder.Services.AddValidatorsFromAssembly(appAssembly);

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    o.AddPolicy("AuthedUser", p => p.RequireAuthenticatedUser());
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Journey API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token. Example: 12345abc (no 'Bearer ' prefix needed)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserDirectory, UserDirectory>();   
builder.Services.AddScoped<AuditableEntitySaveChangesInterceptor>();
builder.Services.AddHttpClient<IEmailQueue, HttpEmailQueue>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
    await initialiser.InitialiseAsync();
    await initialiser.SeedAsync();
}

app.UseCors("fe");


app.UseAuthentication();
app.UseUserProjectionUpsert();
// app.UseUserStatusValidation(); // Disabled - no suspension checking
app.UseAuthorization();


app.MapControllers();
app.MapHub<JourneyHub>("/hubs/journeys");
app.UseMiddleware<ExceptionMiddleware>();


app.Run();
