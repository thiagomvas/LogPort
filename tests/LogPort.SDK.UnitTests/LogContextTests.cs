namespace LogPort.SDK.UnitTests;

public sealed class LogContextTests
{
    [SetUp]
    public void Setup()
    {
        ClearContext();
    }

    [Test]
    public void Push_AddsValueToContext()
    {
        using (LogContext.Push("UserId", 123))
        {
            var current = LogContext.Current;

            Assert.That(current, Is.Not.Null);
            Assert.That(current!.ContainsKey("UserId"));
            Assert.That(current["UserId"], Is.EqualTo(123));
        }
    }

    [Test]
    public void Dispose_RemovesValueFromContext()
    {
        using (LogContext.Push("UserId", 123))
        {
        }

        var current = LogContext.Current;

        Assert.That(current == null || !current.ContainsKey("UserId"));
    }

    [Test]
    public void NestedScopes_AreHandledCorrectly()
    {
        using (LogContext.Push("UserId", 123))
        {
            using (LogContext.Push("RequestId", "abc"))
            {
                var current = LogContext.Current;

                Assert.That(current!["UserId"], Is.EqualTo(123));
                Assert.That(current["RequestId"], Is.EqualTo("abc"));
            }

            var afterInner = LogContext.Current;

            Assert.That(afterInner!["UserId"], Is.EqualTo(123));
            Assert.That(afterInner.ContainsKey("RequestId"), Is.False);
        }

        Assert.That(LogContext.Current, Is.Null);
    }

    [Test]
    public void Push_SameKey_OverridesPreviousValue()
    {
        using (LogContext.Push("UserId", 1))
        {
            using (LogContext.Push("UserId", 2))
            {
                Assert.That(LogContext.Current!["UserId"], Is.EqualTo(2));
            }

            Assert.That(LogContext.Current!["UserId"], Is.EqualTo(1));
        }
    }

    [Test]
    public async Task Context_FlowsAcrossAsyncAwait()
    {
        using (LogContext.Push("UserId", 123))
        {
            await Task.Delay(10);

            Assert.That(LogContext.Current!["UserId"], Is.EqualTo(123));
        }
    }

    [Test]
    public async Task Context_IsolatedBetweenAsyncFlows()
    {
        var task1 = Task.Run(async () =>
        {
            using (LogContext.Push("UserId", 1))
            {
                await Task.Delay(20);
                return LogContext.Current!["UserId"];
            }
        });

        var task2 = Task.Run(async () =>
        {
            using (LogContext.Push("UserId", 2))
            {
                await Task.Delay(10);
                return LogContext.Current!["UserId"];
            }
        });

        var results = await Task.WhenAll(task1, task2);

        Assert.That(results[0], Is.EqualTo(1));
        Assert.That(results[1], Is.EqualTo(2));
    }

    [Test]
    public void Dispose_WhenContextEmpty_DoesNotThrow()
    {
        using (LogContext.Push("Key", "Value"))
        {
        }

        Assert.DoesNotThrow(() =>
        {
            using (LogContext.Push("AnotherKey", "AnotherValue"))
            {
            }
        });
    }

    private static void ClearContext()
    {
        var current = LogContext.Current;
        if (current == null)
            return;

        foreach (var key in new List<string>(current.Keys))
            LogContext.Push(key, "").Dispose();
    }
}