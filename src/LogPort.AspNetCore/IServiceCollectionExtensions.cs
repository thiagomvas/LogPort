using LogPort.Core.Models;
using LogPort.SDK;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

public static class IServiceCollectionExtensions
{
    public static WebApplicationBuilder AddLogPort(this WebApplicationBuilder builder, Action<LogPortConfig>? configure = null)
    {
        var config = LogPortConfig.LoadFromEnvironment();
        configure?.Invoke(config);
        builder.Services.AddSingleton(config);
        
        var client = new LogPortClient(config);
        
        builder.Services.AddSingleton<LogPortClient>(client);
        builder.Logging.AddLogPort(client);
        
        return builder;
    }
    
    public static async Task<WebApplication> UseLogPortAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        var client = app.Services.GetRequiredService<LogPortClient>();
        await client.EnsureConnectedAsync(cancellationToken);
        
        app.Lifetime.ApplicationStopping.Register(() =>
        {
            client.FlushAsync().GetAwaiter().GetResult();
            client.Dispose();
        });
        
        return app;
    }

    public static WebApplication UseLogPort(this WebApplication app)
    {
        return UseLogPortAsync(app).GetAwaiter().GetResult();
    }

    public static ILoggingBuilder AddLogPort(this ILoggingBuilder builder, LogPortClient client)
    {
        builder.AddProvider(new LogPortLoggerProvider(client));
        return builder;
    }
}
