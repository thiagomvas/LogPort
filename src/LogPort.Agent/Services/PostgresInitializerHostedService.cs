using LogPort.Core.Models;
using LogPort.Data.Postgres;
using LogPort.Internal;
using LogPort.Internal.Configuration;

namespace LogPort.Agent.Services;

public class PostgresInitializerHostedService : IHostedService
{
    private readonly LogPortConfig _config;
    private readonly ILogger<PostgresInitializerHostedService> _logger;

    public PostgresInitializerHostedService(LogPortConfig config, ILogger<PostgresInitializerHostedService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Initializing Postgres...");
            var initializer = new DatabaseInitializer(msg => _logger.LogDebug(msg));
            await initializer.InitializeAsync(_config.Postgres.ConnectionString!, cancellation: cancellationToken);
            _logger.LogInformation("Postgres initialized successfully.");
        }
        catch (Exception ex)
        {
            var inner = ex.GetBaseException();
            _logger.LogCritical("Failed to initialize Postgres database: {Message}", inner.Message);
            throw;
        }
    }


    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}