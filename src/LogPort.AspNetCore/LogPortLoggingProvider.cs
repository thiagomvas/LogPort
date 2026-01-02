using LogPort.Core.Models;
using LogPort.SDK;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

internal class LogPortLoggerProvider : ILoggerProvider
{
    private readonly IServiceProvider _sp;
    private readonly LogPortClientConfig _config;
    private LogPortClient? _client;

    public LogPortLoggerProvider(IServiceProvider sp, LogPortClientConfig config)
    {
        _sp = sp;
        _config = config;
    }

    private LogPortClient GetClient()
    {
        return _client ??= _sp.GetRequiredService<LogPortClient>();
    }

    public ILogger CreateLogger(string categoryName)
        => new LogPortLogger(categoryName, GetClient, _config);

    public void Dispose() { }
}