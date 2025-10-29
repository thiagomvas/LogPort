using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using var client = new ClientWebSocket();
await client.ConnectAsync(new Uri("ws://localhost:5000/stream"), CancellationToken.None);
Console.WriteLine("Connected to LogPort. Press Enter to send a log.");

while (true)
{
    Console.ReadLine(); // wait for Enter

    var log = new
    {
        Timestamp = DateTime.UtcNow,
        ServiceName = "auth-api",
        Level = "INFO",
        Message = "User pressed Enter",
        UserId = "1234"
    };

    var json = JsonSerializer.Serialize(log);
    var bytes = Encoding.UTF8.GetBytes(json);

    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

    Console.WriteLine("Log sent!");
}