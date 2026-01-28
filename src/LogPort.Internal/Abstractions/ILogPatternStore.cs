using LogPort.Core.Models;

namespace LogPort.Internal.Abstractions;

public interface ILogPatternStore
{
    Task<long> UpsertAsync(string normalizedMessage,
        ulong patternHash,
        DateTime timestamp,
        string level = "INFO",
        CancellationToken cancellationToken = default);
}