using LogPort.Core.Models;
using LogPort.SDK;

var random = new Random();
var services = new[] { "auth-api", "payment-api", "orders-api", "inventory-api" };
var levels = new[] { "INFO", "WARN", "ERROR", "DEBUG" };
var messages = new[]
{
    "User logged in",
    "Payment processed",
    "Order created",
    "Inventory updated",
    "Failed to authenticate",
    "Timeout while calling external service"
};

using var client = LogPortClient.FromServerUrl("ws://localhost:8080/stream");
await client.EnsureConnectedAsync();

// Send a log every time a key is pressed
while (Console.ReadKey().Key != ConsoleKey.Escape)
{
    var log = new LogEntry
    {
        Timestamp = DateTime.UtcNow,
        ServiceName = services[random.Next(services.Length)],
        Level = levels[random.Next(levels.Length)],
        Message = messages[random.Next(messages.Length)]
    };

    client.Log(log);
    Console.WriteLine($"Sent log: {log.Timestamp} [{log.ServiceName}] {log.Level} - {log.Message}");
    
}

return;
const int LOGS_PER_DAY = 1000;
const int DAYS = 7;

for (int day = 0; day < DAYS; day++)
{
    var date = DateTime.UtcNow.Date.AddDays(-day);
    for (int i = 0; i < LOGS_PER_DAY; i++)
    {
        var log = new LogEntry
        {
            Timestamp = date.AddSeconds(random.Next(0, 86400)),
            ServiceName = services[random.Next(services.Length)],
            Level = levels[random.Next(levels.Length)],
            Message = messages[random.Next(messages.Length)]
        };

        client.Log(log);
        Thread.Sleep(5);
    }
}
