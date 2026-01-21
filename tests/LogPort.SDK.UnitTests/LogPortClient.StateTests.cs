using LogPort.SDK.Events;

namespace LogPort.SDK.UnitTests;

public sealed class LogPortClient_StateTests
{
    [Test]
    public async Task EnsureConnectedAsync_ShouldRaise_Connecting_And_Connected()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        var client = new LogPortClient(new() { AgentUrl = "ws://localhost" }, () => fakeWebSocket);

        var events = new List<LogPortClientStateChangedEventArgs>();
        client.StateChanged += (_, e) => events.Add(e);

        await client.EnsureConnectedAsync();

        Assert.That(events.Count, Is.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(events[0].OldState, Is.EqualTo(LogPortClientState.Disconnected));
            Assert.That(events[0].NewState, Is.EqualTo(LogPortClientState.Connecting));

            Assert.That(events[1].OldState, Is.EqualTo(LogPortClientState.Connecting));
            Assert.That(events[1].NewState, Is.EqualTo(LogPortClientState.Connected));
        });
    }
    [Test]
    public async Task EnsureConnectedAsync_WhenServerOffline_ShouldRaise_Disconnected()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        fakeWebSocket.Server.IsOnline = false;

        var client = new LogPortClient(
            new()
            {
                AgentUrl = "ws://localhost",
                ClientMaxReconnectDelay = TimeSpan.FromMilliseconds(50)
            },
            () => fakeWebSocket
        );

        LogPortClientStateChangedEventArgs lastEvent = null;
        client.StateChanged += (_, e) => lastEvent = e;

        using var cts = new CancellationTokenSource(50);

        await client.EnsureConnectedAsync(cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(lastEvent.OldState, Is.EqualTo(LogPortClientState.Connecting));
            Assert.That(lastEvent.NewState, Is.EqualTo(LogPortClientState.Disconnected));
        });
    }


    [Test]
    public async Task Dispose_ShouldRaise_Stopped_State()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();
        var client = new LogPortClient(new() { AgentUrl = "ws://localhost" }, () => fakeWebSocket);

        LogPortClientStateChangedEventArgs lastEvent = null;
        client.StateChanged += (_, e) => lastEvent = e;

        await client.EnsureConnectedAsync();
        client.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(lastEvent.OldState, Is.EqualTo(LogPortClientState.Connected));
            Assert.That(lastEvent.NewState, Is.EqualTo(LogPortClientState.Stopped));
        });
    }

    [Test]
    public async Task SendFailure_ShouldTransition_To_Degraded()
    {
        var fakeWebSocket = new Fakes.FakeWebSocketClient();

        var client = new LogPortClient(new() { AgentUrl = "ws://localhost", ClientHeartbeatInterval = TimeSpan.FromMilliseconds(10), ClientHeartbeatTimeout = TimeSpan.FromMilliseconds(50) }, () => fakeWebSocket);

        var degradedRaised = false;
        client.StateChanged += (_, e) =>
        {
            if (e.NewState == LogPortClientState.Degraded)
                degradedRaised = true;
        };

        await client.EnsureConnectedAsync();

        client.Log("INFO", "trigger heartbeat/send failure");

        await Utils.WaitUntilAsync(() => degradedRaised);

        Assert.That(degradedRaised, Is.True);
    }
}