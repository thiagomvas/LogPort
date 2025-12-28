using System.Text;

namespace LogPort.SDK;

using System;
using System.IO;
using System.Text;

internal class LogPortTextWriterDecorator : TextWriter
{
    private readonly TextWriter _original;
    private readonly LogPort.SDK.LogPortClient _logger;

    public LogPortTextWriterDecorator(TextWriter original, LogPort.SDK.LogPortClient logger)
    {
        _original = original;
        _logger = logger;
    }

    public override Encoding Encoding => _original.Encoding;

    public override void WriteLine(string? value)
    {
        _original.WriteLine(value);

        if (!string.IsNullOrEmpty(value))
        {
            _logger.Log("Info", value);
        }
    }

    public override void Write(string? value)
    {
        _original.Write(value);
        if (!string.IsNullOrEmpty(value))
        {
            _logger.Log("Info", value);
        }
    }
}