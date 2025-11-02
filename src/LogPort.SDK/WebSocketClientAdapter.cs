using System.Net.WebSockets;

namespace LogPort.SDK;

public sealed class WebSocketClientAdapter : IWebSocketClient
{
    private readonly ClientWebSocket _socket = new();

    public WebSocketState State => _socket.State;

    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        => _socket.ConnectAsync(uri, cancellationToken);

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        => _socket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

    public void Abort() => _socket.Abort();

    public void Dispose() => _socket.Dispose();
}
