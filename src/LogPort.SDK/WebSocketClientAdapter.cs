using System.Net.WebSockets;

namespace LogPort.SDK;

internal sealed class WebSocketClientAdapter : IWebSocketClient
{
    private readonly ClientWebSocket _socket = new();
    private readonly string? _apiToken;

    public WebSocketClientAdapter(string? apiToken = null)
    {
        _apiToken = apiToken;

        if (!string.IsNullOrWhiteSpace(_apiToken))
        {
            _socket.Options.SetRequestHeader("X-API-Token", _apiToken);
        }
    }

    public WebSocketState State => _socket.State;

    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        => _socket.ConnectAsync(uri, cancellationToken);

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        => _socket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

    public async Task CloseConnectionAsync(
        WebSocketCloseStatus closeStatus,
        string statusDescription,
        CancellationToken cancellationToken)
    {
        try
        {
            if (_socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await _socket.CloseAsync(closeStatus, statusDescription, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // normal during shutdown 
        }
        catch (WebSocketException)
        {
            // socket is already closed / aborted 
        }
        catch (ObjectDisposedException)
        {
            // already disposed 
        }
    }


    public void CloseConnection(WebSocketCloseStatus closeStatus, string statusDescription)
    {
        if (_socket.State is not WebSocketState.Open and not WebSocketState.CloseReceived)
            return;

        _socket.CloseAsync(closeStatus, statusDescription, CancellationToken.None).GetAwaiter().GetResult();
    }

    public void Abort() => _socket.Abort();

    public void Dispose() => _socket.Dispose();
}