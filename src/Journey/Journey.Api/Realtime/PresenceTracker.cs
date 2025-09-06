using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Journeys;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Journey.Api.Realtime;

public interface IPresenceTracker
{
    void Add(string userId, string connectionId);
    void Remove(string userId, string connectionId);
    bool IsOnline(string userId);
}

public sealed class PresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _map = new();

    public void Add(string userId, string connectionId)
    {
        var set = _map.GetOrAdd(userId, _ => new HashSet<string>());
        lock (set) set.Add(connectionId);
    }

    public void Remove(string userId, string connectionId)
    {
        if (_map.TryGetValue(userId, out var set))
        {
            lock (set)
            {
                set.Remove(connectionId);
                if (set.Count == 0) _map.TryRemove(userId, out _);
            }
        }
    }

    public bool IsOnline(string userId) => _map.ContainsKey(userId);
}