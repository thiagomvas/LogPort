using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Configuration;
using LogPort.Internal.Services;

var config = new LogPortConfig()
{
    FileTails = [
        new("test-file-service", "/home/thiagomv/logs.txt")
    ]
};

var tailService = new FileTailService(config);

_ = Task.Run(async () =>
{
    await foreach (var (service, line) in tailService.LinesChannel.Reader.ReadAllAsync())
    {
        Console.WriteLine($"[{service}] {line}");
    }
});

_ = tailService.StartAsync();

Console.WriteLine("Tailing logs. Press Enter to exit.");
Console.ReadLine();