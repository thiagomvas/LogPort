using LogPort.Core.Models;
using LogPort.SDK.Filters;

namespace LogPort.SDK.UnitTests;

public class MinimumLogLevelFilterTests
{
    [TestCase("INFO", "DEBUG", false)]
    [TestCase("INFO", "INFO", true)]
    [TestCase("INFO", "WARN", true)]
    [TestCase("ERROR", "WARN", false)]
    public void ShouldSend_RespectsMinimumLevel(
        string minimumLevel,
        string entryLevel,
        bool expected)
    {
        var filter = new MinimumLogLevelFilter(minimumLevel);

        var entry = new LogEntry
        {
            Level = entryLevel
        };

        Assert.That(filter.ShouldSend(entry), Is.EqualTo(expected));
    }
}