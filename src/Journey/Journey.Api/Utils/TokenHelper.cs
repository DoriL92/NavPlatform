using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Journey.Api.Utils;

public static class TokenHelper
{
    public static string NewUrlSafeToken(int bytes = 24) 
    {
        var buf = RandomNumberGenerator.GetBytes(bytes);
        return WebEncoders.Base64UrlEncode(buf);
    }
}