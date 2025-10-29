using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using LogPort.Core.Models;

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

using var client = new ClientWebSocket();
await client.ConnectAsync(new Uri("ws://localhost:5000/stream"), CancellationToken.None);
Console.WriteLine("Connected to LogPort. Press Enter to send a random log.");

while (true)
{
    Console.ReadLine(); // wait for Enter

    var log = new LogEntry
    {
        Timestamp = DateTime.UtcNow,
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

    var json = JsonSerializer.Serialize(log);
    var bytes = Encoding.UTF8.GetBytes(json);

    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

    Console.WriteLine($"Log sent: {log.Level} - {log.ServiceName} - {log.Message}");
}