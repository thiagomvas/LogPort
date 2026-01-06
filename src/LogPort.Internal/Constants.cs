namespace LogPort.Internal;

public static class Constants
{
    public static class Metrics
    {
        public const string LogsProcessed = "logs.processed";
        public const string DockerLogsRead = "docker.logs.read";
        
        public const string BatchSize = "batch.size";
        public static string BuildDockerLogsReadKeyForContainer(string container)
            => $"{DockerLogsRead}.{Normalize(container)}";

        private static string Normalize(string value)
            => value
                .TrimStart('/')
                .ToLowerInvariant();

    }
    
}