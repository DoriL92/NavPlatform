using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Infrastructure.Identity;
public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _ctx;
    public HttpCurrentUser(IHttpContextAccessor ctx) => _ctx = ctx;

    private ClaimsPrincipal? U => _ctx.HttpContext?.User;

    public bool IsAuthenticated => U?.Identity?.IsAuthenticated == true;

    public string? UserId =>
        _ctx.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _ctx.HttpContext?.User?.FindFirst("sub")?.Value
        ?? _ctx.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();

    public string? Email =>
        U?.FindFirst(ClaimTypes.Email)?.Value ??
        U?.FindFirst("email")?.Value ??
        U?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ??
        U?.FindFirst("https://nav-platform.example.com/email")?.Value ??
        _ctx.HttpContext?.Request.Headers["X-User-Email"].FirstOrDefault();

    public string? Name =>
        U?.FindFirst("name")?.Value ??
        U?.FindFirst("nickname")?.Value ??
        U?.FindFirst("given_name")?.Value ??
        U?.FindFirst("https://nav-platform.example.com/name")?.Value ??
        _ctx.HttpContext?.Request.Headers["X-User-Name"].FirstOrDefault() ??
        U?.FindFirst(ClaimTypes.Name)?.Value ?? 
        U?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

    public IEnumerable<string> RolesOrPermissions =>
           U?.FindAll("permissions").Select(c => c.Value) ??
           U?.FindAll("https://nav/roles").Select(c => c.Value) ?? 
           Enumerable.Empty<string>();
}
