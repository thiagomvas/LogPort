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
await client.ConnectAsync();


Console.WriteLine("Connected to LogPort. Press Enter to send a random log.");

while (true)
{
    Console.ReadLine(); 

    var log = new LogEntry
    {
        Timestamp = DateTime.UtcNow.AddHours(random.Next(0, 10) * -1),
        ServiceName = services[random.Next(services.Length)],
        Level = levels[random.Next(levels.Length)],
        Message = messages[random.Next(messages.Length)],
        Metadata = new Dictionary<string, object>
        {
            { "UserId", random.Next(1, 10000) },
            { "SessionId", Guid.NewGuid().ToString() }
        },
        TraceId = Guid.NewGuid().ToString(),
        SpanId = Guid.NewGuid().ToString(),
        Hostname = "localhost",
        Environment = "development"
    };

    client.Log(log);

    Console.WriteLine($"Log queued: {log.Level} - {log.ServiceName} - {log.Message}");
}
