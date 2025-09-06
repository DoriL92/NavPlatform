using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Common.Models;
public sealed class UserDirectory : IUserDirectory
{
    private readonly IApplicationDbContext _db;
    public UserDirectory(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyDictionary<string, string>> ResolveUserIdsByEmailAsync(
        IEnumerable<string> emails, CancellationToken ct)
    {
        var list = (emails ?? Array.Empty<string>())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();

        var users = await _db.EntitySet<User>()
            .Where(u => list.Contains(u.Name.ToLower()))
            .Select(u => new { u.Name, u.Id }) 
            .ToListAsync(ct);

        return users.ToDictionary(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase);
    }
}
