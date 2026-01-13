using System.Collections.Generic;

using LogPort.Core;

using NUnit.Framework;

namespace LogPort.Core.Tests;

[TestFixture]
public sealed class LogNormalizerTests
{
    private LogNormalizer _normalizer = null!;

    [SetUp]
    public void SetUp()
    {
        _normalizer = new LogNormalizer();
    }


    [TestCase("info", LogNormalizer.InfoLevel)]
    [TestCase("INFO", LogNormalizer.InfoLevel)]
    [TestCase("information", LogNormalizer.InfoLevel)]
    [TestCase("warn", LogNormalizer.WarningLevel)]
    [TestCase("WARNING", LogNormalizer.WarningLevel)]
    [TestCase("err", LogNormalizer.ErrorLevel)]
    [TestCase("error", LogNormalizer.ErrorLevel)]
    [TestCase("failed", LogNormalizer.ErrorLevel)]
    [TestCase("fatal", LogNormalizer.FatalLevel)]
    [TestCase("critical", LogNormalizer.FatalLevel)]
    [TestCase("panic", LogNormalizer.FatalLevel)]
    [TestCase("debug", LogNormalizer.DebugLevel)]
    [TestCase("dbg", LogNormalizer.DebugLevel)]
    [TestCase("trace", LogNormalizer.TraceLevel)]
    [TestCase("verbose", LogNormalizer.TraceLevel)]
    public void NormalizeLevel_KnownLevels_ReturnsNormalizedLevel(string input, string expected)
    {
        Assert.That(_normalizer.NormalizeLevel(input), Is.EqualTo(expected));
    }

    [TestCase("unknown")]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void NormalizeLevel_UnknownOrEmpty_ReturnsDefaultLevel(string? input)
    {
        Assert.That(_normalizer.NormalizeLevel(input!), Is.EqualTo(LogNormalizer.DefaultLevel));
    }


    [Test]
    public void NormalizeMessage_Replaces_Metadata()
    {
        var message = "User JohnDoe failed from 127.0.0.1";
        var metadata = new Dictionary<string, object>
        {
            ["user"] = "JohnDoe",
            ["ip"] = "127.0.0.1"
        };

        var result = _normalizer.NormalizeMessage(message, metadata);

        Assert.That(result, Is.EqualTo("User {user} failed from {ip}"));
    }

    [Test]
    public void NormalizeMessage_Normalizes_Timestamp()
    {
        var message = "2025-01-12 14:33:21.456 Request completed";

        var result = _normalizer.NormalizeMessage(message);

        Assert.That(result, Is.EqualTo("{timestamp} Request completed"));
    }

    [Test]
    public void NormalizeMessage_Normalizes_Guid()
    {
        var message = "JobId=3f2504e0-4f89-41d3-9a0c-0305e82c3301";

        var result = _normalizer.NormalizeMessage(message);

        Assert.That(result, Is.EqualTo("JobId={guid}"));
    }

    [Test]
    public void NormalizeMessage_Normalizes_Unix_Path()
    {
        var message = "Failed to open /var/log/nginx/access.log";

        var result = _normalizer.NormalizeMessage(message);

        Assert.That(result, Is.EqualTo("Failed to open {path}"));
    }

    [Test]
    public void NormalizeMessage_Normalizes_Multiple_Numbers()
    {
        var message = "Retry 3 of 10 failed after 2.5 seconds";

        var result = _normalizer.NormalizeMessage(message);

        Assert.That(
            result,
            Is.EqualTo("Retry {number} of {number} failed after {number} seconds")
        );
    }

    [Test]
    public void NormalizeMessage_Is_Deterministic()
    {
        var message = "Job 42 failed at 2025-01-12T14:33:21Z";

        var first = _normalizer.NormalizeMessage(message);
        var second = _normalizer.NormalizeMessage(message);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void ComputePatternHash_Same_Normalized_Message_Produces_Same_Hash()
    {
        var message = "Request 123 failed at 2025-01-12T14:33:21Z";

        var normalized = _normalizer.NormalizeMessage(message);

        var hash1 = LogNormalizer.ComputePatternHash(normalized);
        var hash2 = LogNormalizer.ComputePatternHash(normalized);

        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public void ComputePatternHash_Different_Patterns_Produce_Different_Hashes()
    {
        var msg1 = _normalizer.NormalizeMessage("Request 1 failed");
        var msg2 = _normalizer.NormalizeMessage("Request succeeded");

        Assert.That(
            LogNormalizer.ComputePatternHash(msg1),
            Is.Not.EqualTo(LogNormalizer.ComputePatternHash(msg2))
        );
    }
}