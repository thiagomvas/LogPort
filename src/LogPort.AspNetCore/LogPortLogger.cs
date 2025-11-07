using System.Reflection;
using LogPort.Core.Models;
using LogPort.SDK;
using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

internal class LogPortLogger : ILogger
{
    private static readonly string? _serviceVersion =
        (Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion)?
        .Split('+')[0];

    private readonly string _category;
    private readonly LogPortClient _client;
    private readonly string? _serviceName;

    public LogPortLogger(string category, LogPortClient client, LogPortClientConfig config)
    {
        _category = category;
        _client = client;
        _serviceName = config.ServiceName;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var hostname = Environment.MachineName;
        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = logLevel.ToString(),
            Message = message,
            Metadata = new Dictionary<string, object>
            {
                { "Category", _category },
                { "EventId", eventId.Id },
                { "EventName", eventId.Name ?? string.Empty },
            },
            ServiceName = _serviceName,
            Environment = environment,
            Hostname = hostname
        };
        if (exception is not null)
        {
            entry.Metadata["Exception"] = exception.ToString();
        }
        
        if (!string.IsNullOrEmpty(_serviceVersion))
        {
            entry.Metadata["Version"] = _serviceVersion;
        }
        
        

        _client.Log(entry);
    }
}