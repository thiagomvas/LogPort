using System.Text.Json;
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
    private readonly string _socketPath = "unix:///var/run/docker.sock";
    private readonly LogPortConfig _logPortConfig;

    private readonly List<DockerExtractorConfig> _extractorConfigs = new();

    public DockerLogService(LogQueue logQueue, LogPortConfig config, ILogger<DockerLogService>? logger = null)
    {
        _logger = logger;
        _logQueue = logQueue;
        _socketPath = config.Docker.SocketPath;
        _logPortConfig = config;

        if (!string.IsNullOrWhiteSpace(config.Docker.ExtractorConfigPath))
        {
            try
            {
                var json = File.ReadAllText(config.Docker.ExtractorConfigPath);
                var configs = JsonSerializer.Deserialize<List<DockerExtractorConfig>>(json);
                if (configs != null)
                {
                    _extractorConfigs = configs;
                    _logger?.LogInformation("Loaded {Count} Docker extractor configurations from {Path}",
                        _extractorConfigs.Count, config.Docker.ExtractorConfigPath);
                }
            }
            catch
            {
                _logger?.LogError("Failed to load Docker extractor configurations from {Path}",
                    config.Docker.ExtractorConfigPath);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logPortConfig.Docker.WatchAllContainers)
            _logger?.LogInformation("Initializing Docker log service, watching all containers.");
        else
            _logger?.LogInformation("Initializing Docker log service, watching containers with label com.logport.monitor=true.");
        
        
        
        var client = new DockerClientConfiguration(new Uri(_socketPath)).CreateClient();

        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters
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
            _ = Task.Run(() => StreamContainerLogsAsync(client, container.ID, stoppingToken), stoppingToken);
        }

        _ = Task.Run(() => WatchForNewContainersAsync(client, stoppingToken), stoppingToken);

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

            var config = _extractorConfigs.FirstOrDefault(c => c.ServiceName == containerName);
            if (config != null)
            {
                if (config.ExtractionMode.Equals("regex", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(config.ExtractorRegex))
                {
                    var regex = new System.Text.RegularExpressions.Regex(config.ExtractorRegex);
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        if (!string.IsNullOrWhiteSpace(config.MessageKey) && match.Groups["message"].Success)
                        {
                            message = match.Groups[config.MessageKey].Value;
                        }

                        if (!string.IsNullOrWhiteSpace(config.LogLevelKey) && match.Groups["level"].Success)
                        {
                            level = match.Groups[config.LogLevelKey].Value.ToUpperInvariant();
                        }
                    }
                }
                else if (config.ExtractionMode.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var sanitizedLine = line.Replace("\0", string.Empty);

                        var jsonStart = sanitizedLine.IndexOf('{');
                        if (jsonStart >= 0)
                        {
                            var jsonPart = sanitizedLine.Substring(jsonStart);
                            using var jsonDoc = JsonDocument.Parse(jsonPart);
                            var root = jsonDoc.RootElement;

                            if (!string.IsNullOrWhiteSpace(config.MessageKey) &&
                                root.TryGetProperty(config.MessageKey, out var messageProp))
                            {
                                message = messageProp.GetString() ?? message;
                            }

                            if (!string.IsNullOrWhiteSpace(config.LogLevelKey) &&
                                root.TryGetProperty(config.LogLevelKey, out var levelProp))
                            {
                                level = levelProp.GetString()?.ToUpperInvariant() ?? level;
                            }

                            if (!string.IsNullOrWhiteSpace(config.TimestampKey) &&
                                root.TryGetProperty(config.TimestampKey, out var tsProp))
                            {
                                if (DateTime.TryParse(
                                        tsProp.GetString(),
                                        null,
                                        System.Globalization.DateTimeStyles.AdjustToUniversal |
                                        System.Globalization.DateTimeStyles.AssumeUniversal,
                                        out var ts))
                                {
                                    timestamp = ts;
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                    }
                }
            }

            var logEntry = new LogEntry
            {
                Timestamp = timestamp,
                Message = SanitizeLogMessage(message),
                ServiceName = containerName,
                Level = level,
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