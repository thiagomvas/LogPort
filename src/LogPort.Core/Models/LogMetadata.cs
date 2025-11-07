namespace LogPort.Core.Models;

public class LogMetadata
{
    public string[] LogLevels { get; set; } = [];
    public string[] Environments { get; set; } = [];
    public string[] Services { get; set; } = [];
    public string[] Hostnames { get; set; } = [];
    public Dictionary<string, int> LogCountByLevel { get; set; } = new();
    public Dictionary<string, int> LogCountByService { get; set; } = new();
    public Dictionary<string, int> LogCountByEnvironment { get; set; } = new();
    public Dictionary<string, int> LogCountByHostname { get; set; } = new();
    public long LogCount { get; set; }
}