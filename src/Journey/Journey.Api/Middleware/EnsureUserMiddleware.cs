using System.Security.Claims;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Journey.Api.Middleware;
public class EnsureUserExistsMiddleware
{
    private readonly RequestDelegate _next;
    public EnsureUserExistsMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx, ICurrentUser me, ApplicationDbContext db)
    {
        if (me.IsAuthenticated && !string.IsNullOrEmpty(me.UserId))
        {
            var id = me.UserId!;
            var u = await db.EntitySet<User>().SingleOrDefaultAsync(x => x.Id == id);
            if (u is null)
            {
                u = new User(id, me.Email, me.Name);
                db.EntitySet<User>().Add(u);
            }
            else
            {
                // Update email and name if they're provided and different
                if (!string.IsNullOrEmpty(me.Email) && u.Email != me.Email)
                    u.Email = me.Email;
                if (!string.IsNullOrEmpty(me.Name) && u.Name != me.Name)
                    u.Name = me.Name;
                
                // Update last seen
                u.LastSeenAt = DateTime.UtcNow;
            }
            await db.SaveChangesAsync();
        }

        await _next(ctx);
    }
}
public static class EnsureUserExistsMiddlewareExtensions
{
    public static IApplicationBuilder UseUserProjectionUpsert(this IApplicationBuilder app)
        => app.UseMiddleware<EnsureUserExistsMiddleware>();
}

