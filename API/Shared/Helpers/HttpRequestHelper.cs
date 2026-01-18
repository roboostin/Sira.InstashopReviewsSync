namespace API.Shared.Helpers;

using Microsoft.AspNetCore.Http;
using UAParser;

public static class HttpRequestHelper
{
    public static HttpContext context => new HttpContextAccessor().HttpContext;

    public static string GetClientIp()
    {
        // Try to get IP from X-Forwarded-For (used by proxies)
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        
        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Connection.RemoteIpAddress?.ToString();
        }

        return string.IsNullOrEmpty(ip) ? "Unknown" : ip;
    }

    public static string GetUserAgent()
    {
        return context.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
    public static string GetCountry()
    {
        return context.Request.Headers["CF-IPCountry"].ToString() ?? "Unknown";
    }
    public static string GetTimeZone()
    {
        return context.Request.Headers["TZ"].ToString() ?? "UTC";
   }
    

    public static string GetBrowserInfo()
    {
        var userAgent = GetUserAgent();
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown Browser";
        }

        var parser = Parser.GetDefault();
        var clientInfo = parser.Parse(userAgent);
        return $"{clientInfo.UA.Family} {clientInfo.UA.Major}.{clientInfo.UA.Minor}";
    }
}
