namespace LogPort.SDK;

/// <summary>
/// Represents the connection and operational state of a <see cref="LogPortClient"/> instance.
/// </summary>
/// <remarks>
/// The client transitions between these states as it establishes a connection to the server,
/// handles transient failures, and shuts down. State changes are exposed via the
/// <c>LogPortClient.StateChanged</c> event.
///
/// State transitions are deterministic and occur synchronously when the internal state changes.
/// </remarks>
public enum LogPortClientState
{
    /// <summary>
    /// The client is not connected to the server and is not currently attempting to connect.
    /// </summary>
    /// <remarks>
    /// This is the initial state of a newly created client. The client may transition to
    /// <see cref="Connecting"/> when <c>EnsureConnectedAsync</c> is called, or remain in this
    /// state after a failed connection attempt.
    /// </remarks>
    Disconnected,

    /// <summary>
    /// The client is actively attempting to establish a connection to the server.
    /// </summary>
    /// <remarks>
    /// This state is entered when a connection attempt begins. If the attempt succeeds,
    /// the client transitions to <see cref="Connected"/>. If it fails, the client transitions
    /// back to <see cref="Disconnected"/> and may retry depending on configuration.
    /// </remarks>
    Connecting,

    /// <summary>
    /// The client is connected to the server and able to send log entries.
    /// </summary>
    /// <remarks>
    /// While in this state, queued log entries are transmitted asynchronously. A loss of
    /// connectivity or a protocol failure will cause a transition to
    /// <see cref="Disconnected"/> or <see cref="Degraded"/>.
    /// </remarks>
    Connected,

    /// <summary>
    /// The client is partially connected but experiencing degraded communication.
    /// </summary>
    /// <remarks>
    /// This state typically indicates that the underlying connection is still open but
    /// heartbeat checks or send operations are failing. The client may attempt recovery
    /// and transition back to <see cref="Connected"/> or fall back to
    /// <see cref="Disconnected"/>.
    /// </remarks>
    Degraded,

    /// <summary>
    /// The client has been explicitly stopped and can no longer be used.
    /// </summary>
    /// <remarks>
    /// This state is entered after the client has been disposed. Once in this state,
    /// no further connections will be attempted and no events will be raised.
    /// </remarks>
    Stopped
}
