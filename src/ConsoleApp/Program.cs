using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LogPort.Core.Models;
using LogPort.Postgres;

var connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=logport";

await DatabaseInitializer.InitializeAsync(connectionString);

var repo = new PostgresLogRepository(connectionString);

var log = new LogEntry
{
    Timestamp = DateTime.UtcNow,
    ServiceName = "auth-api",
    Level = "INFO",
    Message = "Test log entry",
    Metadata = new Dictionary<string, object>
    {
        { "userId", 123 },
        { "action", "login" }
    },
    Hostname = "localhost",
    Environment = "dev"
};

await repo.AddLogAsync(log);
Console.WriteLine("Inserted log entry.");

var queryParams = new LogQueryParameters
{
    ServiceName = "auth-api",
    Page = 1,
    PageSize = 10
};

var logs = await repo.GetLogsAsync(queryParams);
var count = await repo.CountLogsAsync(queryParams);

Console.WriteLine($"Found {count} logs for service {queryParams.ServiceName}:");

foreach (var l in logs)
{
    Console.WriteLine($"{l.Timestamp:u} [{l.Level}] {l.ServiceName} - {l.Message} | Metadata: {JsonSerializer.Serialize(l.Metadata)}");
}