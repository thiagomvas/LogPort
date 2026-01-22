namespace LogPort.Internal.Abstractions;

public abstract class JobBase
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Cron { get;  }
    public abstract bool Enabled { get; }

    public abstract Task ExecuteAsync();
}
