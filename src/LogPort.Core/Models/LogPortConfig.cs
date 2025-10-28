namespace LogPort.Core.Models;

public class LogPortConfig
{
    public string ElasticUri { get; set; } = null!;
    public string DefaultIndex { get; set; } = "logs";
    public string? ElasticUsername { get; set; }
    public string? ElasticPassword { get; set; }
    
    public static LogPortConfig LoadFromEnvironment()
    {
        return new LogPortConfig
        {
            ElasticUri = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_URI") ?? throw new InvalidOperationException("LOGPORT_ELASTIC_URI environment variable is not set."),
            DefaultIndex = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_DEFAULT_INDEX") ?? "logs",
            ElasticUsername = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_USERNAME"),
            ElasticPassword = Environment.GetEnvironmentVariable("LOGPORT_ELASTIC_PASSWORD")
        };
    }
}