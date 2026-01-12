using LogPort.Core.Models;
using LogPort.SDK;

string agentUrl = args.Length > 0 ? args[0] : "localhost:8080";
string defaultService = args.Length > 1 ? args[1] : "dev-service";

var config = new LogPortClientConfig { AgentUrl = agentUrl, ServiceName = defaultService };
var client = new LogPortClient(config, null, null, new LogPortConsoleLogger());
await client.EnsureConnectedAsync();
Console.WriteLine($"Connected to LogPort Agent at {agentUrl}");

var random = new Random();
var levels = new[] { "INFO", "WARN", "ERROR", "DEBUG" };
var services = new[] { defaultService, "auth", "db", "cache" };
var predefinedMessages = new[]
{
    "User logged in", "Database query executed", "Cache miss occurred", "Error connecting to service",
    "Background job started", "Background job completed", "Request timed out", "Processing message queue"
};

Console.WriteLine("Type logs manually, '/populate-24h' to generate 24h of logs, or 'exit' to quit.");

while (true)
{
    var line = Console.ReadLine()?.Trim();
    if (line?.ToLower() == "exit") break;

    if (line == "/populate-24h")
    {
        Console.WriteLine("Populating 24 hours of logs...");
        var logs = new List<LogEntry>();

        for (int hourOffset = 0; hourOffset < 24; hourOffset++)
        {
            int logsThisHour = random.Next(100, 301); // 100-300 logs per hour

            for (int i = 0; i < logsThisHour; i++)
            {
                var level = levels[random.Next(levels.Length)];
                var service = services[random.Next(services.Length)];
                var message = predefinedMessages[random.Next(predefinedMessages.Length)];

                var timestamp = DateTime.UtcNow.AddHours(-hourOffset)
                                                .AddMinutes(-random.Next(60))
                                                .AddSeconds(-random.Next(60));

                logs.Add(new LogEntry
                {
                    Level = level,
                    ServiceName = service,
                    Message = message,
                    Timestamp = timestamp
                });
            }
        }

        foreach (var entry in logs)
        {
            client.Log(entry);
        }

        Console.WriteLine($"Generated {logs.Count} logs over the past 24 hours.");
    }
    else if (!string.IsNullOrWhiteSpace(line))
    {
        // Manual log
        client.Log("INFO", line);
    }
    else
    {
        // Generate a single random log
        var level = levels[random.Next(levels.Length)];
        var service = services[random.Next(services.Length)];
        var message = predefinedMessages[random.Next(predefinedMessages.Length)];

        var entry = new LogEntry
        {
            Level = level,
            ServiceName = service,
            Message = message,
            Timestamp = DateTime.UtcNow.AddHours(-random.NextDouble() * 24)
        };

        client.Log(entry);
        Console.WriteLine($"Generated [{level}] {service}: {message}");
    }
}

Console.WriteLine("Log generator exited.");