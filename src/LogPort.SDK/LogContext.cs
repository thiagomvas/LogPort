namespace LogPort.SDK;

public static class LogContext
{
    internal static string? TraceId => System.Diagnostics.Activity.Current?.TraceId.ToString();
    internal static string? SpanId => System.Diagnostics.Activity.Current?.SpanId.ToString();

    private static readonly AsyncLocal<ContextFrame?> _current = new();

    public static IReadOnlyDictionary<string, object>? Current
        => _current.Value?.Data;

    public static IDisposable Push(string key, object value)
    {
        var previous = _current.Value;

        var next = previous != null
            ? new ContextFrame(previous)
            : new ContextFrame();

        next.Data[key] = value;
        _current.Value = next;

        return new PopScope(previous);
    }

    private sealed class PopScope : IDisposable
    {
        private readonly ContextFrame? _previous;
        private bool _disposed;

        public PopScope(ContextFrame? previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _current.Value = _previous;
        }
    }

    private sealed class ContextFrame
    {
        public Dictionary<string, object> Data { get; }

        public ContextFrame()
        {
            Data = new Dictionary<string, object>();
        }

        public ContextFrame(ContextFrame parent)
        {
            Data = new Dictionary<string, object>(parent.Data);
        }
    }
}