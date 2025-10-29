namespace LogPort.Core.Models;

public class LogPortConfig
{
    public bool UseElasticSearch { get; set; } = false;
    public string ElasticUri { get; set; } = null!;
    public string DefaultIndex { get; set; } = "logs";
    public string? ElasticUsername { get; set; }
    public string? ElasticPassword { get; set; }
    
    public bool UsePostgres { get; set; } = false;
    public string? PostgresHost { get; set; }
    public int? PostgresPort { get; set; }
    public string? PostgresDatabase { get; set; }
    public string? PostgresUsername { get; set; }
    public string? PostgresPassword { get; set; }
    
    
    public string PostgresConnectionString =>
        $"Host={PostgresHost ?? "localhost"};" +
        $"Port={PostgresPort ?? 5432};" +
        $"Database={PostgresDatabase ?? "logport"};" +
        $"Username={PostgresUsername ?? "postgres"};" +
        $"Password={PostgresPassword ?? "postgres"};";
    public static LogPortConfig LoadFromEnvironment()
    {
        return new LogPortConfig
        {
            UseElasticSearch = bool.TryParse(Environment.GetEnvironmentVariable("LOGPORT_USE_ELASTICSEARCH"), out var useEs) && useEs,
            ElasticUri = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_URI") ?? throw new InvalidOperationException("LOGPORT_ELASTIC_URI environment variable is not set."),
            DefaultIndex = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_DEFAULT_INDEX") ?? "logs",
            ElasticUsername = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_USERNAME"),
            ElasticPassword = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_PASSWORD"),
            UsePostgres = bool.TryParse(Environment.GetEnvironmentVariable("LOGPORT_USE_POSTGRES"), out var usePg) && usePg,
            PostgresHost = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_HOST"),
            PostgresPort = int.TryParse(Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_PORT"), out var port) ? port : null,
            PostgresDatabase = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_DATABASE"),
            PostgresUsername = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_USERNAME"),
            PostgresPassword = Environment.GetEnvironmentVariable("LOGPORT_POSTGRES_PASSWORD"),
        };
    }
}