using LogPort.Core.Models;

namespace LogPort.Internal.Abstractions;

public interface ILogMetadataStore
{
    Task<LogMetadata> GetAsync(DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken cancellationToken = default);
}