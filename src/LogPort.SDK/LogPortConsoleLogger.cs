using System;

namespace LogPort.SDK;
/// <summary>
/// Console-based implementation of <see cref="ILogPortLogger"/> that writes
/// color-coded log messages to standard output.
/// </summary>
public sealed class LogPortConsoleLogger : ILogPortLogger
{
    /// <summary>
    /// Synchronization object to ensure thread-safe console writes.
    /// </summary>
    private static readonly object _lock = new();

    /// <summary>
    /// Writes a debug-level log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">An optional exception associated with the log entry.</param>
    public void Debug(string message, Exception? ex = null)
    {
        Write("DEBUG", message, ConsoleColor.Gray, ex);
    }

    /// <summary>
    /// Writes an informational log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">An optional exception associated with the log entry.</param>
    public void Info(string message, Exception? ex = null)
    {
        Write("INFO", message, ConsoleColor.Green, ex);
    }

    /// <summary>
    /// Writes a warning-level log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">An optional exception associated with the log entry.</param>
    public void Warn(string message, Exception? ex = null)
    {
        Write("WARN", message, ConsoleColor.Yellow, ex);
    }

    /// <summary>
    /// Writes an error-level log message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="ex">An optional exception associated with the log entry.</param>
    public void Error(string message, Exception? ex = null)
    {
        Write("ERROR", message, ConsoleColor.Red, ex);
    }

    /// <summary>
    /// Writes a formatted, colorized log message to the console.
    /// </summary>
    /// <param name="level">The log level label.</param>
    /// <param name="message">The log message.</param>
    /// <param name="color">The console color used for the log level.</param>
    /// <param name="ex">An optional exception to include in the output.</param>
    private static void Write(string level, string message, ConsoleColor color, Exception? ex)
    {
        lock (_lock)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");

            if (ex != null)
                Console.WriteLine($"    Exception: {ex.GetType().Name} - {ex.Message}");

            Console.ForegroundColor = originalColor;
        }
    }
}
