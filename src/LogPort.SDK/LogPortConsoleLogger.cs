using System;

namespace LogPort.SDK;

internal class LogPortConsoleLogger : ILogPortLogger
{
    private static readonly object _lock = new();

    public void Debug(string message, Exception? ex = null)
    {
        Write("DEBUG", message, ConsoleColor.Gray, ex);
    }

    public void Info(string message, Exception? ex = null)
    {
        Write("INFO", message, ConsoleColor.Green, ex);
    }

    public void Warn(string message, Exception? ex = null)
    {
        Write("WARN", message, ConsoleColor.Yellow, ex);
    }

    public void Error(string message, Exception? ex = null)
    {
        Write("ERROR", message, ConsoleColor.Red, ex);
    }

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