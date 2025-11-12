using LogPort.Core;

namespace Logport.Core.Tests;

public class LogNormalizerTests
{
    [Test]
    public void NormalizeLevel_KnownLevels_ReturnsNormalizedLevel()
    {
        var normalizer = new LogNormalizer();

        Assert.That(normalizer.NormalizeLevel("info"), Is.EqualTo(LogNormalizer.InfoLevel));
        Assert.That(normalizer.NormalizeLevel("WARNING"), Is.EqualTo(LogNormalizer.WarningLevel));
        Assert.That(normalizer.NormalizeLevel("Err"), Is.EqualTo(LogNormalizer.ErrorLevel));
        Assert.That(normalizer.NormalizeLevel("fatal"), Is.EqualTo(LogNormalizer.FatalLevel));
        Assert.That(normalizer.NormalizeLevel("Debug"), Is.EqualTo(LogNormalizer.DebugLevel));
        Assert.That(normalizer.NormalizeLevel("TRACE"), Is.EqualTo(LogNormalizer.TraceLevel));
    }
    
    [Test]
    public void NormalizeLevel_UnknownLevel_ReturnsDefaultLevel()
    {
        var normalizer = new LogNormalizer();

        Assert.That(normalizer.NormalizeLevel("unknown"), Is.EqualTo(LogNormalizer.DefaultLevel));
        Assert.That(normalizer.NormalizeLevel(""), Is.EqualTo(LogNormalizer.DefaultLevel));
        Assert.That(normalizer.NormalizeLevel(null), Is.EqualTo(LogNormalizer.DefaultLevel));
    }
}