using LogPort.Core.Models;

namespace LogPort.Internal.Abstractions;

public abstract class BaseLogEntryExtractor
{
    public abstract bool TryExtract(ReadOnlySpan<char> BaseLogEntryExtractor, out LogEntry result);
}