namespace LogPort.Core.Models;

using System;

public class LogPortConfig
{
    public ElasticConfig Elastic { get; set; } = new();
    public PostgresConfig Postgres { get; set; } = new();
    public DockerConfig Docker { get; set; } = new();

    public uint Port { get; set; } = 8080;
    public string AgentUrl { get; set; } = "http://localhost:8080";
    public int BatchSize { get; set; } = 100;
    public int FlushIntervalMs { get; set; } = 250;
    public TimeSpan ClientMaxReconnectDelay { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ClientHeartbeatInterval { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan ClientHeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public static LogPortConfig LoadFromEnvironment()
    {
        var config = new LogPortConfig();

        config.Port = GetEnvUInt("LOGPORT_PORT", 8080);
        config.AgentUrl = Environment.GetEnvironmentVariable("LOGPORT_AGENT_URL") ?? $"http://localhost:{config.Port}";
        config.BatchSize = GetEnvInt("LOGPORT_BATCH_SIZE", 100);
        config.FlushIntervalMs = GetEnvInt("LOGPORT_FLUSH_INTERVAL_MS", 250);

        config.ClientMaxReconnectDelay =
            TimeSpan.FromMilliseconds(GetEnvInt("LOGPORT_CLIENT_MAX_RECONNECT_DELAY_MS", 30000));
        config.ClientHeartbeatInterval =
            TimeSpan.FromMilliseconds(GetEnvInt("LOGPORT_CLIENT_HEARTBEAT_INTERVAL_MS", 10000));
        config.ClientHeartbeatTimeout =
            TimeSpan.FromMilliseconds(GetEnvInt("LOGPORT_CLIENT_HEARTBEAT_TIMEOUT_MS", 10000));

        // Elastic
        config.Elastic.Use = GetEnvBool("LOGPORT_USE_ELASTICSEARCH");
        config.Elastic.Uri = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_URI")
                             ?? (config.Elastic.Use
                                 ? throw new InvalidOperationException("LOGPORT_ELASTIC_URI is required")
                                 : null);
        config.Elastic.DefaultIndex = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_DEFAULT_INDEX") ?? "logs";
        config.Elastic.Username = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_USERNAME");
        config.Elastic.Password = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_PASSWORD");

        // Postgres
        config.Postgres.Use = GetEnvBool("LOGPORT_USE_POSTGRES");
        config.Postgres.Host = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_HOST") ?? "localhost";
        config.Postgres.Port = GetEnvInt("LOGPORT_POSTGRES_PORT", 5432);
        config.Postgres.Database = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_DATABASE") ?? "logport";
        config.Postgres.Username = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_USERNAME") ?? "postgres";
        config.Postgres.Password = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_PASSWORD") ?? "postgres";
        config.Postgres.PartitionLength = GetEnvInt("LOGPORT_POSTGRES_PARTITION_LENGTH", 1);

        // Docker
        config.Docker.Use = GetEnvBool("LOGPORT_USE_DOCKER");
        config.Docker.SocketPath = Environment.GetEnvironmentVariable("LOGPORT_DOCKER_SOCKET_PATH") ?? "unix:///var/run/docker.sock";
        config.Docker.ExtractorConfigPath = Environment.GetEnvironmentVariable("LOGPORT_DOCKER_EXTRACTOR_CONFIG_PATH");
        
        
        if (!config.Postgres.Use && !config.Elastic.Use)
            throw new InvalidOperationException("At least one storage backend must be enabled.");
        return config;
    }

    private static bool GetEnvBool(string key, bool defaultValue = false)
    {
        return bool.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;
    }

    private static int GetEnvInt(string key, int defaultValue)
    {
        return int.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;
    }

    private static uint GetEnvUInt(string key, uint defaultValue)
    {
        return uint.TryParse(Environment.GetEnvironmentVariable(key), out var val) ? val : defaultValue;
    }

    public class ElasticConfig
    {
        public bool Use { get; set; } = false;
        public string? Uri { get; set; }
        public string DefaultIndex { get; set; } = "logs";
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class PostgresConfig
    {
        public bool Use { get; set; } = false;
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "logport";
        public string Username { get; set; } = "postgres";
        public string Password { get; set; } = "postgres";

        public string ConnectionString =>
            $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";

        public int PartitionLength { get; set; } = 1;
    }

    public class DockerConfig
    {
        public bool Use { get; set; } = false;
        public string SocketPath { get; set; } = "unix:///var/run/docker.sock";
        public string? ExtractorConfigPath { get; set; }
    }

}
