using LogPort.Core.Models;
using LogPort.SDK;
using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;


public class LogPortLoggerProvider : ILoggerProvider
{
    private readonly LogPortClient _client;
    private readonly LogPortClientConfig _config;

    public LogPortLoggerProvider(LogPortClient client, LogPortClientConfig config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public ILogger CreateLogger(string categoryName) => new LogPortLogger(categoryName, _client, _config);

    public void Dispose() { }
}