using LogPort.Core.Models;
using LogPort.Data.Postgres.Commands;
using LogPort.Internal.Abstractions;

namespace LogPort.Data.Postgres;

public sealed class PostgresLogMetadataStore : ILogMetadataStore
{
    private readonly IDbSessionFactory _sessionFactory;

    public PostgresLogMetadataStore(IDbSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<LogMetadata> GetAsync(DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken cancellationToken = default)
    {
        var command = GetLogMetadataCommand.Create(from, to);

        await using var session = _sessionFactory.Create();
        await session.OpenAsync(cancellationToken);

        var dto = await session.QuerySingleAsync<LogMetadataDto>(command, cancellationToken);

        return dto.ToMetadata();
    }
}