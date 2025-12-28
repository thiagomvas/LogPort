using System.Net.WebSockets;

namespace LogPort.SDK;

public interface IWebSocketClient : IDisposable
{
    WebSocketState State { get; }
    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
    Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    Task CloseConnectionAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
    void CloseConnection(WebSocketCloseStatus closeStatus, string statusDescription);
    void Abort();
}