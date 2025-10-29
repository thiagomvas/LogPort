namespace LogPort.Core.Models;

using System;

public class LogPortConfig
{
    public ElasticConfig Elastic { get; set; } = new();
    public PostgresConfig Postgres { get; set; } = new();
    
    public uint Port { get; set; } = 8080;
    public string AgentUrl { get; set; }

    public static LogPortConfig LoadFromEnvironment()
    {
        var config = new LogPortConfig();

        config.Port = GetEnvUInt("LOGPORT_PORT", 8080);
        config.AgentUrl = Environment.GetEnvironmentVariable("LOGPORT_AGENT_URL") ?? $"http://localhost:{config.Port}";

        // Elastic
        config.Elastic.Use = GetEnvBool("LOGPORT_USE_ELASTICSEARCH");
        config.Elastic.Uri = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_URI") 
                             ?? (config.Elastic.Use ? throw new InvalidOperationException("LOGPORT_ELASTIC_URI is required") : null);
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
    }
}
