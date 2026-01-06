using LogPort.SDK;

string agentUrl = args.Length > 0 ? args[0] : "ws://localhost:8080";
string defaultService = args.Length > 1 ? args[1] : "dev-service";

var config = new LogPortClientConfig { AgentUrl = agentUrl, ServiceName =  defaultService };

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

Console.WriteLine("Type logs manually, or press Enter to generate random logs. Type 'exit' to quit.");

while (true)
{
    var line = Console.ReadLine();
    if (line?.Trim().ToLower() == "exit")
        break;

    if (!string.IsNullOrWhiteSpace(line))
    {
        // Manual log
        client.Log("INFO", line);
    }
    else
    {
        // Generate random log
        var level = levels[random.Next(levels.Length)];
        var service = services[random.Next(services.Length)];
        var message = predefinedMessages[random.Next(predefinedMessages.Length)];
        client.Log(level, message);
        Console.WriteLine($"Generated [{level}] {service}: {message}");
    }
}

Console.WriteLine("Log generator exited.");