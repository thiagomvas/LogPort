using LogPort.Core.Models;
using LogPort.Internal.DSL;
using LogPort.SDK;


var config = new LogPortClientConfig { AgentUrl = "localhost:8080", ServiceName = "ConsoleApp", ApiSecret = "123" };
var client = new LogPortClient(config, null, null, new LogPortConsoleLogger());

await client.EnsureConnectedAsync();

while (Console.ReadKey().Key != ConsoleKey.Escape)
{
    client.Log(new LogEntry() { Message = "Hello, world ", Timestamp = DateTime.UtcNow });
    Console.WriteLine("Logged");
}