using LogPort.Core.Models;
using LogPort.SDK.Filters;

namespace LogPort.SDK.UnitTests;

public class WhitelistLogLevelFilterTests
{
    [Test]
    public void ShouldSend_ReturnsTrue_ForWhitelistedLevel()
    {
        var filter = new WhitelistLogLevelFilter("INFO", "ERROR");

        var entry = new LogEntry { Level = "INFO" };

        Assert.That(filter.ShouldSend(entry), Is.True);
    }

    [Test]
    public void ShouldSend_ReturnsFalse_ForNonWhitelistedLevel()
    {
        var filter = new WhitelistLogLevelFilter("ERROR");

        var entry = new LogEntry { Level = "DEBUG" };

        Assert.That(filter.ShouldSend(entry), Is.False);
    }
}