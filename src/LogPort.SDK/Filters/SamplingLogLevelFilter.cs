using System.Security.Cryptography;
using System.Text;

using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

public sealed class SamplingLogLevelFilter : ILogLevelFilter
{
    private readonly Dictionary<string, double> _rates;
    private readonly bool _deterministic;
    private readonly Random _random = new Random();

    public SamplingLogLevelFilter(
        bool deterministic = true)
    {
        _rates = new Dictionary<string, double>(
            StringComparer.OrdinalIgnoreCase);

        _deterministic = deterministic;
    }

    public void SetRate(string level, double rate)
    {
        _rates[level] = rate;
    }

    public bool ShouldSend(LogEntry entry)
    {
        if (!_rates.TryGetValue(entry.Level, out var rate))
            return true;

        if (rate >= 1.0)
            return true;

        if (rate <= 0.0)
            return false;

        return _deterministic
            ? DeterministicSample(entry, rate)
            : _random.NextDouble() < rate;
    }

    private static bool DeterministicSample(LogEntry entry, double rate)
    {
        var key =
            entry.TraceId ??
            entry.Message ??
            string.Empty;

        var hash = ComputeHash(key);
        var normalized = hash / (double)ulong.MaxValue;

        return normalized < rate;
    }

    private static ulong ComputeHash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);

        return BitConverter.ToUInt64(hash, 0);
    }
}