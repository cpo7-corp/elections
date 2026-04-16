namespace Elections.Api.Core;

public static class HttpContextExtensions
{
    public static string GetClientIp(this HttpContext? httpContext)
    {
        if (httpContext == null) return null;

        var cfIp = httpContext.Request.Headers["cf-connecting-ip"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfIp))
        {
            return cfIp.Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString()?.Trim() ?? "unknown";
    }
}
