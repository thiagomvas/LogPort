using Docker.DotNet;
using Docker.DotNet.Models;
using LogPort.Core;
using LogPort.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogPort.Internal.Docker;

public class DockerLogService : BackgroundService
{
    private readonly ILogger<DockerLogService>? _logger;
    private readonly LogQueue _logQueue;
    
    public DockerLogService(LogQueue logQueue, ILogger<DockerLogService>? logger = null)
    {
        _logger = logger;
        _logQueue = logQueue;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        
        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                { "label", new Dictionary<string, bool> { { "com.logport.monitor=true", true } } }
            }
        });
        
        foreach (var container in containers)
        {
            _logger?.LogInformation("Starting log stream for container {ContainerId} ({ContainerNames})", container.ID, string.Join(", ", container.Names));
            _ = Task.Run(() => StreamContainerLogsAsync(client, container.ID, stoppingToken), stoppingToken);
        }
    }
    
    private async Task StreamContainerLogsAsync(DockerClient client, string containerId, CancellationToken stoppingToken)
    {
        var parameters = new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Follow = true,
            Timestamps = true,
            Tail = "0"
        };

        using var logStream = await client.Containers.GetContainerLogsAsync(containerId, parameters, stoppingToken);
        using var reader = new StreamReader(logStream);

        while (!stoppingToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (line == null)
                break;

            var logEntry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Message = line,
                ServiceName = containerId,
                Level = "INFO",
                Hostname = Environment.MachineName,
                Environment = "docker"
            };
            _logger?.LogInformation("Docker log: {LogMessage}", line);

            //_logQueue.Enqueue(logEntry);
        }
    }
}