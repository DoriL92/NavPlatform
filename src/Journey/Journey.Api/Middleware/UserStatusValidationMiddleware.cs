using System.Security.Claims;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Journey.Api.Middleware;

public class UserStatusValidationMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ICurrentUser currentUser, ApplicationDbContext db)
    {
        // Skip validation for anonymous requests or admin endpoints (admins can manage user statuses)
        if (!currentUser.IsAuthenticated || 
            context.Request.Path.StartsWithSegments("/api/admin") ||
            context.Request.Path.StartsWithSegments("/internal"))
        {
            await _next(context);
            return;
        }

        var userId = currentUser.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await db.EntitySet<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null && user.Status != UserStatus.Active)
            {
                var statusMessage = user.Status switch
                {
                    UserStatus.Suspended => "Your account has been suspended. Please contact support for assistance.",
                    UserStatus.Deactivated => "Your account has been deactivated. Please contact support to reactivate your account.",
                    _ => "Your account access has been restricted. Please contact support."
                };

                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Account Access Restricted",
                    message = statusMessage,
                    status = user.Status.ToString()
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }
        }

        await _next(context);
    }
}

public static class UserStatusValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseUserStatusValidation(this IApplicationBuilder app)
        => app.UseMiddleware<UserStatusValidationMiddleware>();
}


