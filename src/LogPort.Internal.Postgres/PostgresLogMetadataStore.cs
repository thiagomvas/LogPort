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

    public async Task<LogMetadata> GetAsync(DateTimeOffset? from = null, DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        await using var session = _sessionFactory.Create();
        await session.OpenAsync(cancellationToken);

        var levels = await session.QueryAsync<string>(GetLogLevelsCommand.Create(), cancellationToken);
        var services = await session.QueryAsync<string>(GetServicesCommand.Create(), cancellationToken);
        var environments = await session.QueryAsync<string>(GetEnvironmentsCommand.Create(), cancellationToken);
        var hostnames = await session.QueryAsync<string>(GetEnvironmentsCommand.Create(), cancellationToken);

        var logsPerLevel =
            await session.QueryAsync<(string level, int count)>(GetLogsPerLevelCommand.Create(from, to),
                cancellationToken);
        var logsPerService =
            await session.QueryAsync<(string service, int count)>(GetLogsPerServiceCommand.Create(from, to),
                cancellationToken);
        var logsByHostname =
            await session.QueryAsync<(string hostname, int count)>(GetLogsByHostnameCommand.Create(from, to),
                cancellationToken);
        var logsByEnvironment =
            await session.QueryAsync<(string environment, int count)>(GetLogsByEnvironmentCommand.Create(from, to),
                cancellationToken);

        var queryParams = new LogQueryParameters();
        if (from.HasValue)
            queryParams.From = from?.DateTime;
        if (to.HasValue)
            queryParams.To = to?.DateTime ?? DateTime.UtcNow;

        var count = await session.ExecuteScalarAsync<long>(CountLogsCommand.Create(queryParams), cancellationToken);

        var result = new LogMetadata()
        {
            LogLevels = levels.ToArray(),
            Services = services.ToArray(),
            Environments = environments.ToArray(),
            Hostnames = hostnames.ToArray(),
            LogCountByLevel = logsPerLevel.ToDictionary(x => x.level, x => x.count),
            LogCountByService = logsPerService.ToDictionary(x => x.service, x => x.count),
            LogCountByEnvironment = logsByEnvironment.ToDictionary(x => x.environment, x => x.count),
            LogCountByHostname = logsByHostname.ToDictionary(x => x.hostname, x => x.count),
            LogCount = count
        };

        return result;
    }
}