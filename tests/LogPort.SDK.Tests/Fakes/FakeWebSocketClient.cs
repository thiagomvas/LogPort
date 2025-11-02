using System.Net.WebSockets;
using LogPort.Core.Models;

namespace LogPort.SDK.Tests.Fakes;

public class FakeWebSocketClient : IWebSocketClient
{
    public WebSocketState State { get; private set; } = WebSocketState.None;
    public readonly FakeServer Server = new FakeServer();
    
    public bool ConnectCalled { get; private set; }
    public bool SendCalled { get; private set; }
    public bool AbortCalled { get; private set; }
    public bool DisposeCalled { get; private set; }
    
    public int SentMessagesCount { get; private set; }

    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        ConnectCalled = true;
        State = WebSocketState.Open;
        return Task.CompletedTask;
    }

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        if (!Server.IsOnline)
        {
            throw new WebSocketException("Server is offline");
        }
        SendCalled = true;
        SentMessagesCount++;
        var message = System.Text.Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        var entry = System.Text.Json.JsonSerializer.Deserialize<LogEntry>(message);
        Server.ReceiveLog(entry);
        return Task.CompletedTask;
    }

    public void Abort()
    {
        AbortCalled = true;
        State = WebSocketState.Closed;
    }

    public void Dispose()
    {
        DisposeCalled = true;
    }
}
