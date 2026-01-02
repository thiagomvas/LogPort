using LogPort.SDK;

using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

internal sealed class MicrosoftLoggerAdapter<T> : ILogPortLogger
{
    private readonly ILogger<T> _logger;

    public MicrosoftLoggerAdapter(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void Debug(string message, Exception? ex = null) => _logger.LogDebug(ex, message);
    public void Info(string message, Exception? ex = null) => _logger.LogInformation(ex, message);
    public void Warn(string message, Exception? ex = null) => _logger.LogWarning(ex, message);
    public void Error(string message, Exception? ex = null) => _logger.LogError(ex, message);
}