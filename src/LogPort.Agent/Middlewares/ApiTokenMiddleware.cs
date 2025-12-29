using System.Security.Cryptography;
using System.Text;

using LogPort.Internal;

namespace LogPort.Agent.Middlewares;

public sealed class ApiTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _token;

    public ApiTokenMiddleware(RequestDelegate next, LogPortConfig config)
    {
        _next = next;
        _token = config.ApiSecret;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrEmpty(_token))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("API token is not configured.");
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-API-Token", out var provided))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!SecureEquals(provided.ToString(), _token))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context);
    }

    private static bool SecureEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}