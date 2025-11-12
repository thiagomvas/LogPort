namespace LogPort.SDK;

public interface ILogPortLogger
{
    void Debug(string message, Exception? ex = null);
    void Info(string message, Exception? ex = null);
    void Warn(string message, Exception? ex = null);
    void Error(string message, Exception? ex = null);
}