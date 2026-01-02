using System.Net.WebSockets;

namespace LogPort.SDK;

/// <summary>
/// Abstraction over a WebSocket client used for communicating with a LogPort agent.
/// </summary>
public interface IWebSocketClient : IDisposable
{
    /// <summary>
    /// Gets the current state of the WebSocket connection.
    /// </summary>
    WebSocketState State { get; }

    /// <summary>
    /// Establishes a WebSocket connection to the specified URI.
    /// </summary>
    /// <param name="uri">The remote WebSocket endpoint.</param>
    /// <param name="cancellationToken">Token used to cancel the connection attempt.</param>
    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

    /// <summary>
    /// Sends data over the WebSocket connection.
    /// </summary>
    /// <param name="buffer">The data to send.</param>
    /// <param name="messageType">The type of WebSocket message.</param>
    /// <param name="endOfMessage">
    /// Indicates whether this is the final fragment of the message.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the send operation.</param>
    Task SendAsync(
        ArraySegment<byte> buffer,
        WebSocketMessageType messageType,
        bool endOfMessage,
        CancellationToken cancellationToken);

    /// <summary>
    /// Closes the WebSocket connection asynchronously using the specified close status.
    /// </summary>
    /// <param name="closeStatus">The reason for closing the connection.</param>
    /// <param name="statusDescription">A description of the close reason.</param>
    /// <param name="cancellationToken">Token used to cancel the close operation.</param>
    Task CloseConnectionAsync(
        WebSocketCloseStatus closeStatus,
        string statusDescription,
        CancellationToken cancellationToken);

    /// <summary>
    /// Closes the WebSocket connection synchronously using the specified close status.
    /// </summary>
    /// <param name="closeStatus">The reason for closing the connection.</param>
    /// <param name="statusDescription">A description of the close reason.</param>
    void CloseConnection(
        WebSocketCloseStatus closeStatus,
        string statusDescription);

    /// <summary>
    /// Immediately terminates the WebSocket connection without performing a close handshake.
    /// </summary>
    void Abort();
}
