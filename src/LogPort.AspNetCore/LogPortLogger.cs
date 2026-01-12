using System.Reflection;

using LogPort.Core.Models;
using LogPort.SDK;

using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

internal class LogPortLogger : ILogger
{
    private readonly string _category;
    private readonly Func<LogPortClient> _clientFactory;
    private readonly LogPortClientConfig _config;

    public LogPortLogger(string category, Func<LogPortClient> clientFactory, LogPortClientConfig config)
    {
        _category = category;
        _clientFactory = clientFactory;
        _config = config;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var client = _clientFactory(); // lazy resolution
        var message = formatter(state, exception);

        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = logLevel.ToString(),
            Message = message,
            Metadata = new()
            {
                ["Category"] = _category,
                ["EventId"] = eventId.Id,
                ["EventName"] = eventId.Name ?? string.Empty
            },
            ServiceName = _config.ServiceName,
            Environment = _config.Environment,
            Hostname = _config.Hostname
        };

        if (exception != null)
            entry.Metadata["Exception"] = exception.ToString();

        if (state is IReadOnlyList<KeyValuePair<string, object>> stateProperties)
        {
            foreach (var kvp in stateProperties)
            {
                if (kvp.Key != "{OriginalFormat}")
                {
                    entry.Metadata[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }
            }
        }

        client.Log(entry);
    }
}