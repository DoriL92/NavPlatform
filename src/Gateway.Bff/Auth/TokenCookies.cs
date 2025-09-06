namespace Gateway.Bff.Auth;

public static class TokenCookies
{
    public const string Access = "bff.at";
    public const string Refresh = "bff.rt";

    public static CookieOptions Secure(HttpContext ctx) => new()
    {
        HttpOnly = true,
        Secure = false,                // set to true (Always) when running under HTTPS
        SameSite = SameSiteMode.Lax,
        Path = "/"
    };
}