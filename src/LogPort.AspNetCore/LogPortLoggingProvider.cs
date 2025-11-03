using LogPort.Core.Models;
using LogPort.SDK;
using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;


public class LogPortLoggerProvider : ILoggerProvider
{
    private readonly LogPortClient _client;

    public LogPortLoggerProvider(LogPortClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ILogger CreateLogger(string categoryName) => new LogPortLogger(categoryName, _client);

    public void Dispose() { }
}