using LogPort.Core.Models;
using LogPort.SDK;
using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

internal class LogPortLogger : ILogger
{
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

        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = logLevel.ToString(),
            Message = message,
            Metadata = new Dictionary<string, object>
            {
                { "Category", _category },
                { "EventId", eventId.Id },
                { "EventName", eventId.Name ?? string.Empty }
            },
            ServiceName = _serviceName
        };

        _client.Log(entry);
    }
}