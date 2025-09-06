using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Infrastructure;
public sealed class HttpEmailQueue : IEmailQueue
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _user;

    public HttpEmailQueue(HttpClient http, IConfiguration cfg, IApplicationDbContext db)
        => (_http, _cfg, _db) = (http, cfg, db);

    private async Task<string?> EmailOf(string? userId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(userId)) return null;
        
        var user = await _db.EntitySet<User>()
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);
            
        return user;
    }


    public async Task EnqueueJourneyUpdateAsync(string userId, int journeyId, string kind, CancellationToken ct)
        => await Post("journey-update", new { Email = await EmailOf(userId, ct), JourneyId = journeyId, Kind = kind }, ct);

    public async Task EnqueueJourneySharedAsync(string rid, int jid, string actor, CancellationToken ct)
        => await Post("journey-shared", new { Email = await EmailOf(rid, ct), JourneyId = jid, ActorUserId = actor }, ct);

    public async Task EnqueueJourneyUnsharedAsync(string rid, int jid, string actor, CancellationToken ct)
        => await Post("journey-unshared", new { Email = await EmailOf(rid, ct), JourneyId = jid, ActorUserId = actor }, ct);

    public async Task EnqueueDailyGoalAsync(string uid, DateOnly date, double totalKm, CancellationToken ct)
        => await Post("daily-goal", new { Email = await EmailOf(uid, ct), Date = date, TotalKm = totalKm }, ct);

    private async Task Post(string path, object payload, CancellationToken ct)
    {
        var baseUrl = _cfg["NotificationApi:BaseUrl"]!.TrimEnd('/');
        var res = await _http.PostAsJsonAsync($"{baseUrl}/api/notify/{path}", payload, ct);
        res.EnsureSuccessStatusCode();
    }
}