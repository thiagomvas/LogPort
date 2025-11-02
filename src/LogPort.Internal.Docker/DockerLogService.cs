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
        
        _ = Task.Run(() => WatchForNewContainersAsync(client, stoppingToken), stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
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
        
        // Get container name, default to id if not found
        var inspect = await client.Containers.InspectContainerAsync(containerId, stoppingToken);
        var containerName = inspect.Name?.TrimStart('/') ?? containerId;
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
                Message = SanitizeLogMessage(line),
                ServiceName = containerName,
                Level = "INFO",
                Hostname = Environment.MachineName,
                Environment = "docker"
            };
            _logQueue.Enqueue(logEntry);
        }
    }
    
    private async Task WatchForNewContainersAsync(DockerClient client, CancellationToken stoppingToken)
    {
        await client.System.MonitorEventsAsync(
            new ContainerEventsParameters(),
            new Progress<Message>(async message =>
            {
                if (message.Type == "container" && message.Action == "start")
                {
                    try
                    {
                        var inspect = await client.Containers.InspectContainerAsync(message.ID, stoppingToken);
                        if (inspect.Config?.Labels?.TryGetValue("com.logport.monitor", out var val) == true && val == "true")
                        {
                            _logger?.LogInformation("New container detected: {ContainerId} ({Name})",
                                inspect.ID, inspect.Name);
                            _ = Task.Run(() => StreamContainerLogsAsync(client, inspect.ID, stoppingToken), stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error inspecting container {ContainerId}", message.ID);
                    }
                }
            }),
            stoppingToken
        );
    }
    
    private string SanitizeLogMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        message = message.Replace("\0", string.Empty);


        return message;
    }


}