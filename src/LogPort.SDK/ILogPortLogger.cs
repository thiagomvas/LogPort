namespace LogPort.SDK;

/// <summary>
/// Defines a minimal logging abstraction used internally by LogPort components.
/// </summary>
public interface ILogPortLogger
{
    /// <summary>
    /// Logs a debug-level message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">An optional associated exception.</param>
    void Debug(string message, Exception? ex = null);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">An optional associated exception.</param>
    void Info(string message, Exception? ex = null);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">An optional associated exception.</param>
    void Warn(string message, Exception? ex = null);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">An optional associated exception.</param>
    void Error(string message, Exception? ex = null);
}