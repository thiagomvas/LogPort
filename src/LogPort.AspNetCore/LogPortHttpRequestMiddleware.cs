using LogPort.Core.Models;
using LogPort.SDK;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

internal sealed class LogPortHttpRequestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LogPortClient _client;
    private readonly LogPortClientConfig _config;

    public LogPortHttpRequestMiddleware(RequestDelegate next, LogPortClient client, LogPortClientConfig config)
    {
        _next = next;
        _client = client;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = "INFO",
            Message = $"{request.Method} {request.Path}",
            Metadata = new Dictionary<string, object>
            {
                { "Method", request.Method },
                { "Path", request.Path },
                { "QueryString", request.QueryString.ToString() },
                { "Headers", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()) },
                { "RemoteIpAddress", context.Connection.RemoteIpAddress?.ToString() ?? "Unknown" }
            },
            ServiceName = _config.ServiceName,
            Environment = _config.Environment ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Hostname = _config.Hostname ?? Environment.MachineName
        };

        var bodyStream = new MemoryStream();
        if (request.ContentLength > 0 && request.Body.CanRead)
        {
            var originalBody = request.Body;
            await request.Body.CopyToAsync(bodyStream);
            bodyStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(bodyStream, leaveOpen: true);
            var bodyText = await reader.ReadToEndAsync();
            logEntry.Metadata.Add("Body", bodyText);
            bodyStream.Seek(0, SeekOrigin.Begin);
            request.Body = bodyStream;
        }

        _client.Log(logEntry);

        await _next(context);
    }

}