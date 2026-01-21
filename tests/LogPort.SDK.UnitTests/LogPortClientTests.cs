using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace LogPort.SDK.UnitTests;

public class LogPortClientTests
{

    [Test]
    public async Task LogMessage_ShouldSend_LogEntry()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        var client = new LogPortClient(new() { AgentUrl = "ws://localhost" }, () => fakeWebSocket);

        const string logMessage = "Test log message";
        const string logLevel = "INFO";

        await client.EnsureConnectedAsync();
        client.Log(logLevel, logMessage);

        await Utils.WaitUntilAsync(() => fakeWebSocket.Server.ReceivedLogs.Count == 1);

        Assert.Multiple(() =>
        {
            Assert.That(fakeWebSocket.SentMessagesCount, Is.EqualTo(1));
            Assert.That(fakeWebSocket.Server.ReceivedLogs.Count, Is.EqualTo(1));
        });

        var receivedLog = fakeWebSocket.Server.ReceivedLogs[0];
        Assert.Multiple(() =>
        {
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

        await Utils.WaitUntilAsync(() => fakeWebSocket.Server.ReceivedLogs.Count == 1);

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

        var client = new LogPortClient(
            new() { AgentUrl = "ws://localhost" },
            () => fakeWebSocket
        );

        const string logMessage = "Test log message while server is offline";
        const string logLevel = "WARN";

        _ = client.EnsureConnectedAsync();

        client.Log(logLevel, logMessage);

        await Task.Delay(100);

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

        await Utils.WaitUntilAsync(() => fakeWebSocket.Server.ReceivedLogs.Count == 1);

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

    [TestCase("http://localhost:8080", "ws://localhost:8080/agent/stream")]
    [TestCase("https://example.com:5000", "wss://example.com:5000/agent/stream")]
    [TestCase("localhost:8080", "ws://localhost:8080/agent/stream")]
    [TestCase("ws://localhost:8080", "ws://localhost:8080/agent/stream")]
    [TestCase("wss://secure.com", "wss://secure.com/agent/stream")]
    public void LogPortClient_UrlValidation_ShouldCorrectlyTransformUrls(string inputUrl, string expectedUrl)
    {
        var config = new LogPortClientConfig
        {
            AgentUrl = inputUrl
        };

        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        var client = new LogPortClient(config, () => fakeWebSocket);

        Assert.That(client.ServerUri.ToString(), Is.EqualTo(expectedUrl));
    }
    
    [Test]
    public async Task FlushAsync_WhenQueueNotFlushedBeforeTimeout_ShouldReturnFalse()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        fakeWebSocket.Server.IsOnline = false;

        var config = new LogPortClientConfig() { AgentUrl = "ws://localhost", AutomaticReconnect = false, };

        var client = new LogPortClient(
            config,
            () => fakeWebSocket
        );
        await client.EnsureConnectedAsync();

        client.Log("INFO", "will never be sent");

        var result = await client.FlushAsync(TimeSpan.FromMilliseconds(100));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task FlushAsync_WhenQueueFlushedBeforeTimeout_ShouldReturnTrue()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        fakeWebSocket.Server.IsOnline = true;

        var client = new LogPortClient(
            new() { AgentUrl = "ws://localhost" },
            () => fakeWebSocket
        );

        await client.EnsureConnectedAsync();

        client.Log("INFO", "will be sent");

        var result = await client.FlushAsync(TimeSpan.FromSeconds(1));

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(fakeWebSocket.Server.ReceivedLogs.Count, Is.EqualTo(1));
        });
    }


}