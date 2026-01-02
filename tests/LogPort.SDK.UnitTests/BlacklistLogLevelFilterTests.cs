using LogPort.Core.Models;
using LogPort.SDK.Filters;

namespace LogPort.SDK.UnitTests;

public class BlacklistLogLevelFilterTests
{
    [Test]
    public void ShouldSend_ReturnsFalse_ForBlacklistedLevel()
    {
        var filter = new BlacklistLogLevelFilter("DEBUG");

        var entry = new LogEntry { Level = "DEBUG" };

        Assert.That(filter.ShouldSend(entry), Is.False);
    }

    [Test]
    public void ShouldSend_ReturnsTrue_ForNonBlacklistedLevel()
    {
        var filter = new BlacklistLogLevelFilter("DEBUG");

        var entry = new LogEntry { Level = "INFO" };

        Assert.That(filter.ShouldSend(entry), Is.True);
    }
}