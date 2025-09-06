using System.Security.Claims;
using System.Security.Principal;
using CleanArchitecture.Application.Common.Interfaces;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUser(IHttpContextAccessor http) => _http = http;

    private ClaimsPrincipal? Principal => _http.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? UserId =>
         _http.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault()
        ?? _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _http.HttpContext?.User.FindFirst("sub")?.Value;

    public string? Name =>
        Principal?.Identity?.Name ??
        Get("name") ??
        Get(ClaimTypes.Name);

    public string? Email =>
        Get("email") ??
        Get(ClaimTypes.Email);

    private string? Get(string type) => Principal?.FindFirst(type)?.Value;

    public IEnumerable<string> RolesOrPermissions =>
        Principal.FindAll("roles").Select(c => c.Value)
        ?? Enumerable.Empty<string>();
}
