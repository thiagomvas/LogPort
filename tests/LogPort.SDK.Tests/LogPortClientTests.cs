using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LogPort.SDK.Tests;

public class LogPortClientTests
{
    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000, int pollIntervalMs = 25)
    {
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                throw new TimeoutException("Condition not met within timeout.");
            await Task.Delay(pollIntervalMs);
        }
    }

    [Test]
    public async Task LogMessage_ShouldSend_LogEntry()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        var client = new LogPortClient(new() { AgentUrl = "ws://localhost" }, () => fakeWebSocket);

        const string logMessage = "Test log message";
        const string logLevel = "INFO";

        await client.EnsureConnectedAsync();
        client.Log(logLevel, logMessage);

        await WaitUntilAsync(() => fakeWebSocket.Server.ReceivedLogs.Count == 1);

        Assert.Multiple(() =>
        {
            Assert.That(fakeWebSocket.SentMessagesCount, Is.EqualTo(1));
            Assert.That(fakeWebSocket.Server.ReceivedLogs.Count, Is.EqualTo(1));
        });

        var receivedLog = fakeWebSocket.Server.ReceivedLogs[0];
        Assert.Multiple(() =>
        {
            Assert.That(receivedLog.Level, Is.EqualTo(logLevel));
            Assert.That(receivedLog.Message, Is.EqualTo(logMessage));
        });
    }

    [Test]
    public async Task LogEntry_ShouldSend_LogEntry()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        var client = new LogPortClient(new() { AgentUrl = "ws://localhost" }, () => fakeWebSocket);

        var logEntry = new Core.Models.LogEntry
        {
            Level = "ERROR",
            Message = "An error occurred",
            Timestamp = DateTime.UtcNow
        };

        await client.EnsureConnectedAsync();
        client.Log(logEntry);

        await WaitUntilAsync(() => fakeWebSocket.Server.ReceivedLogs.Count == 1);

        Assert.Multiple(() =>
        {
            Assert.That(fakeWebSocket.SentMessagesCount, Is.EqualTo(1));
            Assert.That(fakeWebSocket.Server.ReceivedLogs.Count, Is.EqualTo(1));
        });

        var receivedLog = fakeWebSocket.Server.ReceivedLogs[0];
        Assert.Multiple(() =>
        {
            Assert.That(receivedLog.Message, Is.EqualTo(logEntry.Message));
        });
    }

    [Test]
    public async Task Process_WhenServerIsOffline_ShouldNotThrow()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        fakeWebSocket.Server.IsOnline = false;
        var client = new LogPortClient(new() { AgentUrl = "ws://localhost" }, () => fakeWebSocket);

        const string logMessage = "Test log message while server is offline";
        const string logLevel = "WARN";

        await client.EnsureConnectedAsync();
        client.Log(logLevel, logMessage);

        await Task.Delay(100); // short delay is fine — no network activity expected

        Assert.Multiple(() =>
        {
            Assert.That(fakeWebSocket.SentMessagesCount, Is.EqualTo(0));
            Assert.That(fakeWebSocket.Server.ReceivedLogs.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task Logs_ShouldBeSent_AfterReconnection()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        var client = new LogPortClient(new() { AgentUrl = "ws://localhost" }, () => fakeWebSocket);

        await client.EnsureConnectedAsync();

        fakeWebSocket.Server.IsOnline = false;

        const string logMessage = "Test after reconnect";
        const string logLevel = "INFO";

        client.Log(logLevel, logMessage);

        await Task.Delay(100); // allow send attempt to fail

        Assert.That(fakeWebSocket.SentMessagesCount, Is.EqualTo(0));

        fakeWebSocket.Server.IsOnline = true;

        await WaitUntilAsync(() => fakeWebSocket.Server.ReceivedLogs.Count == 1);

        Assert.Multiple(() =>
        {
            Assert.That(fakeWebSocket.SentMessagesCount, Is.GreaterThan(0));
            Assert.That(fakeWebSocket.Server.ReceivedLogs.Count, Is.EqualTo(1));
        });

        var receivedLog = fakeWebSocket.Server.ReceivedLogs[0];
        Assert.Multiple(() =>
        {
            Assert.That(receivedLog.Message, Is.EqualTo(logMessage));
        });
    }
}
