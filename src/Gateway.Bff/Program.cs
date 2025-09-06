using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Xml.Linq;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

var rolesClaimType = "https://nav-platform.example.com/roles";

builder.Services.PostConfigure<OpenIdConnectOptions>(Auth0.AspNetCore.Authentication.Auth0Constants.AuthenticationScheme, o =>
{
    o.TokenValidationParameters ??= new TokenValidationParameters();
    o.TokenValidationParameters.RoleClaimType = rolesClaimType; 

    o.Events ??= new OpenIdConnectEvents();
    o.Events.OnTokenValidated = ctx =>
    {
        var id = (ClaimsIdentity)ctx.Principal!.Identity!;

        var sub = id.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(sub) && !id.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            id.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));

        var rawRoleClaims = id.FindAll(rolesClaimType).ToList();

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in rawRoleClaims)
        {
            var v = c.Value?.Trim();
            if (string.IsNullOrEmpty(v)) continue;

            if (v.StartsWith("["))                     
            {
                try
                {
                    var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(v) ?? Array.Empty<string>();
                    foreach (var r in roles)
                        if (seen.Add(r)) id.AddClaim(new Claim(ClaimTypes.Role, r));
                }
                catch { }
            }
            else                                       
            {
                if (seen.Add(v)) id.AddClaim(new Claim(ClaimTypes.Role, v));
            }
        }


        return Task.CompletedTask;
    };
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("Spa", p =>
        p.WithOrigins("http://localhost:4200") // or https://localhost:4200 if you switch to HTTPS
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(b =>
    {
        b.AddRequestTransform(async ctx =>
        {
            var userId = ctx.HttpContext.User.FindFirst("sub")?.Value
                         ?? ctx.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                ctx.ProxyRequest.Headers.Remove("X-User-Id");
                ctx.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
            }

            // Forward email and name as headers for additional reliability
            var email = ctx.HttpContext.User.FindFirst("email")?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                ctx.ProxyRequest.Headers.Remove("X-User-Email");
                ctx.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Email", email);
            }

            var name = ctx.HttpContext.User.FindFirst("name")?.Value ?? 
                      ctx.HttpContext.User.FindFirst("nickname")?.Value ?? 
                      ctx.HttpContext.User.FindFirst("given_name")?.Value;
            if (!string.IsNullOrEmpty(name))
            {
                ctx.ProxyRequest.Headers.Remove("X-User-Name");
                ctx.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Name", name);
            }

            var token = await ctx.HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(token))
            {
                ctx.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        });
    });

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = cfg["Auth0:Domain"];
        options.ClientId = cfg["Auth0:ClientId"];
        options.ClientSecret = cfg["Auth0:ClientSecret"];
        options.CallbackPath = "/callback";
        options.Scope = "openid profile email"; // ← This is the key fix!
    })
   .WithAccessToken(options =>
     {
     // MUST match the API Identifier you created in Auth0
     options.Audience = cfg["Auth0:Audience"]; 
     options.UseRefreshTokens = true;
    });

builder.Services.PostConfigureAll<CookieAuthenticationOptions>(o =>
{
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.None;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();


app.UseCors("Spa");
app.UseAuthentication();
app.UseAuthorization();



app.MapGet("/auth/login", async ctx =>
{
    var returnUrl = ctx.Request.Query["returnUrl"].FirstOrDefault() ?? "http://localhost:4200";
    await ctx.ChallengeAsync(Auth0Constants.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = returnUrl
    });
});

app.MapGet("/auth/me", async (HttpContext ctx, IHttpClientFactory httpClientFactory) =>
{
    if (!(ctx.User.Identity?.IsAuthenticated ?? false))
        return Results.Unauthorized();

    var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? ctx.User.FindFirst("sub")?.Value;

    // Check user suspension status from Journey API
    try
    {
        var httpClient = httpClientFactory.CreateClient();
        var token = await ctx.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        var apiBaseUrl = ctx.RequestServices.GetRequiredService<IConfiguration>()["ApiBaseUrl"] ?? "http://localhost:5000";
        var response = await httpClient.GetAsync($"{apiBaseUrl}/api/users/status/{userId}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var statusResponse = JsonSerializer.Deserialize<UserStatusResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (statusResponse?.Status == "Suspended")
            {
                return Results.Json(new
                {
                    error = "account_suspended",
                    message = "Your account has been suspended. Please contact support.",
                    suspended = true
                }, statusCode: 403);
            }
        }
    }
    catch (Exception ex)
    {
        // If we can't check status, allow through (fail open)
        // You might want to fail closed in production
    }

    return Results.Ok(new
    {
        userId,                                         // <-- convenient alias
        sub = userId,                                 // same value
        name = ctx.User.FindFirst("name")?.Value ?? ctx.User.FindFirst("nickname")?.Value ?? ctx.User.FindFirst("given_name")?.Value,
        email = ctx.User.FindFirst("email")?.Value,
        roles = ctx.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray(),
        suspended = false
    });
});

app.MapGet("/auth/token", async (HttpContext ctx) =>
{
    var token = await ctx.GetTokenAsync("access_token");
    return string.IsNullOrEmpty(token)
        ? Results.Unauthorized()
        : Results.Ok(new { access_token = token });
}).RequireAuthorization();

app.MapGet("/auth/logout", async ctx =>
{
    var cfg = ctx.RequestServices.GetRequiredService<IConfiguration>();

    var domain = cfg["Auth0:Domain"];      // e.g. dev-xxxxx.us.auth0.com
    var clientId = cfg["Auth0:ClientId"];
    var returnUrl = ctx.Request.Query["returnUrl"].FirstOrDefault()
                    ?? "http://localhost:4200/login?logout=1";

    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    var logoutUrl =
        $"https://{domain}/v2/logout?client_id={clientId}&returnTo={Uri.EscapeDataString(returnUrl)}";

    ctx.Response.Redirect(logoutUrl);
});
app.MapReverseProxy().RequireCors("Spa"); ;

app.MapControllers();
app.Run();
// DTO for user status response
public record UserStatusResponse(string Status);

