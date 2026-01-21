namespace LogPort.SDK.Events;

public sealed class LogPortClientStateChangedEventArgs : EventArgs
{
    public LogPortClientState OldState { get; init; }
    public LogPortClientState NewState { get; init; }

    public LogPortClientStateChangedEventArgs(LogPortClientState oldState, LogPortClientState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}
