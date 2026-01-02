using LogPort.Core.Models;
using LogPort.SDK.Filters;

namespace LogPort.SDK.UnitTests;

public class SamplingLogLevelFilterTests
{
    [Test]
    public void ShouldSend_ReturnsFalse_WhenRateIsZero()
    {
        var filter = new SamplingLogLevelFilter(deterministic: true);
        filter.SetRate("INFO", 0);

        var entry = new LogEntry { Level = "INFO" };

        Assert.That(filter.ShouldSend(entry), Is.False);
    }

    [Test]
    public void ShouldSend_ReturnsTrue_WhenRateIsOne()
    {
        var filter = new SamplingLogLevelFilter(deterministic: true);
        filter.SetRate("INFO", 1);

        var entry = new LogEntry { Level = "INFO" };

        Assert.That(filter.ShouldSend(entry), Is.True);
    }

    [Test]
    public void ShouldSend_UsesDefaultRate_WhenLevelNotConfigured()
    {
        var filter = new SamplingLogLevelFilter(deterministic: true);
        filter.SetDefaultRate(0);

        var entry = new LogEntry { Level = "WARN" };

        Assert.That(filter.ShouldSend(entry), Is.False);
    }
}