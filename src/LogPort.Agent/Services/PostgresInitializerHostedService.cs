using LogPort.Core.Models;
using LogPort.Data.Postgres;

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
            await DatabaseInitializer.InitializeAsync(_config.Postgres.ConnectionString);
            _logger.LogInformation("Postgres initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Postgres database");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
