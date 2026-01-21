namespace LogPort.SDK.Events;

/// <summary>
/// Provides data for the <c>LogPortClient.StateChanged</c> event, which is raised whenever
/// the <see cref="LogPortClient"/> transitions between connection states.
/// </summary>
/// <remarks>
/// This event is raised synchronously at the moment the internal client state changes.
/// Consumers can use it to react to connection lifecycle events such as initial connection,
/// reconnection attempts, transient failures, and shutdown.
///
/// Typical state transitions include:
/// <list type="bullet">
/// <item><description><see cref="LogPortClientState.Disconnected"/> to <see cref="LogPortClientState.Connecting"/></description></item>
/// <item><description><see cref="LogPortClientState.Connecting"/> to <see cref="LogPortClientState.Connected"/></description></item>
/// <item><description><see cref="LogPortClientState.Connecting"/> to <see cref="LogPortClientState.Disconnected"/></description></item>
/// <item><description><see cref="LogPortClientState.Connected"/> to <see cref="LogPortClientState.Degraded"/></description></item>
/// <item><description><see cref="LogPortClientState.Connected"/> to <see cref="LogPortClientState.Disconnected"/></description></item>
/// </list>
///
/// The event is not raised if the state remains unchanged.
/// </remarks>
public sealed class LogPortClientStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous state of the <see cref="LogPortClient"/> before the transition occurred.
    /// </summary>
    public LogPortClientState OldState { get; init; }

    /// <summary>
    /// Gets the new state of the <see cref="LogPortClient"/> after the transition occurred.
    /// </summary>
    public LogPortClientState NewState { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogPortClientStateChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldState">The state the client was previously in.</param>
    /// <param name="newState">The state the client has transitioned to.</param>
    public LogPortClientStateChangedEventArgs(LogPortClientState oldState, LogPortClientState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}
