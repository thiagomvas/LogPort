using System.Text.Json;

using Docker.DotNet;
using Docker.DotNet.Models;

using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Internal.Configuration;
using LogPort.Internal.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogPort.Internal.Docker;

public class DockerLogService : BackgroundService
{
    private readonly ILogger<DockerLogService>? _logger;
    private readonly LogQueue _logQueue;
    private readonly DockerClient _client;
    private readonly LogPortConfig _logPortConfig;
    private readonly LogEntryExtractionPipeline _extractionPipeline;

    public DockerLogService(
        LogQueue logQueue,
        LogPortConfig config,
        DockerClient client,
        LogEntryExtractionPipeline pipeline,
        ILogger<DockerLogService>? logger = null)

    {
        _logger = logger;
        _logQueue = logQueue;
        _logPortConfig = config;
        _client = client;
        _extractionPipeline = pipeline;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logPortConfig.Docker.WatchAllContainers)
            _logger?.LogInformation("Initializing Docker log service, watching all containers.");
        else
            _logger?.LogInformation("Initializing Docker log service, watching containers with label com.logport.monitor=true.");

        var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = false,
            Filters = _logPortConfig.Docker.WatchAllContainers
                ? new Dictionary<string, IDictionary<string, bool>>() // include all running containers
                : new Dictionary<string, IDictionary<string, bool>>
                {
                    { "label", new Dictionary<string, bool> { { "com.logport.monitor=true", true } } }
                }
        });


        foreach (var container in containers)
        {
            _logger?.LogInformation("Starting log stream for container {ContainerId} ({ContainerNames})", container.ID,
                string.Join(", ", container.Names));
            _ = Task.Run(() => StreamContainerLogsAsync(_client, container.ID, stoppingToken), stoppingToken);
        }

        _ = Task.Run(() => WatchForNewContainersAsync(_client, stoppingToken), stoppingToken);

        _logger?.LogInformation(
            "Docker log service initialized, watching {ContainerCount} existing containers.",
            containers.Count
        );


        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task StreamContainerLogsAsync(DockerClient client, string containerId,
        CancellationToken stoppingToken)
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

            string message = line;
            string level = "INFO";
            DateTime timestamp = DateTime.UtcNow;

            var logEntry = new LogEntry
            {
                Timestamp = timestamp,
                Message = SanitizeLogMessage(message),
                ServiceName = containerName,
                Level = level,
                Hostname = Environment.MachineName,
                Environment = "docker"
            };

            if (_extractionPipeline.TryExtract(containerName, message, out var result))
            {
                logEntry.Message = SanitizeLogMessage(result.Message);
                logEntry.Level = result.Level;
                logEntry.Timestamp = result.Timestamp;
            }

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
                        var inspect = await client.Containers.InspectContainerAsync(message.Actor.ID, stoppingToken);
                        var shouldWatch = _logPortConfig.Docker.WatchAllContainers;

                        if (!shouldWatch)
                        {
                            shouldWatch = inspect.Config?.Labels?.TryGetValue("com.logport.monitor", out var val) == true &&
                                          val == "true";
                        }

                        if (shouldWatch)
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