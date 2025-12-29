using System.Security.Cryptography;
using System.Text;

using LogPort.Internal;

namespace LogPort.Agent.Middlewares;

public sealed class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _user;
    private readonly string _pass;

    public BasicAuthMiddleware(RequestDelegate next, LogPortConfig _config)
    {
        _next = next;
        _user = _config.AdminLogin;
        _pass = _config.AdminPassword;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_pass))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Admin credentials are not configured.");
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            !authHeader.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(context);
            return;
        }

        try
        {
            var encoded = authHeader.ToString()["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));

            var parts = decoded.Split(':', 2);
            if (parts.Length != 2 ||
                !SecureEquals(parts[0], _user) ||
                !SecureEquals(parts[1], _pass))
            {
                Challenge(context);
                return;
            }
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        await _next(context);
    }

    private static void Challenge(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = @"Basic realm=""Analytics""";
    }

    private static bool SecureEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}