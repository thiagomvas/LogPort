using LogPort.Core.Models;
using LogPort.SDK;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogPort.AspNetCore;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures the <see cref="LogPortClient"/> with the ASP.NET Core dependency injection container.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> used to configure the application.</param>
    /// <param name="configure">Optional delegate to customize the <see cref="LogPortClientConfig"/> before initialization.</param>
    /// <returns>The same <see cref="WebApplicationBuilder"/> instance for chaining.</returns>
    /// <remarks>
    /// This method only registers the services, they must be initialized by calling either <see cref="UseLogPortAsync"/> or <see cref="UseLogPort"/>.
    /// By default, it'll load the configuration from environment variables before calling the <paramref name="configure"/> action.
    /// </remarks>
    public static WebApplicationBuilder AddLogPort(this WebApplicationBuilder builder, Action<LogPortClientConfig>? configure = null)
    {
        var config = LogPortClientConfig.LoadFromEnvironment();
        configure?.Invoke(config);
        builder.Services.AddSingleton(config);
        
        var client = new LogPortClient(config);
        
        builder.Services.AddSingleton<LogPortClient>(client);
        builder.Logging.AddLogPort(client);
        
        return builder;
    }
    
    /// <summary>
    /// Connects the <see cref="LogPortClient"/> asynchronously and ensures proper disposal when the application stops.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    /// <param name="cancellationToken">Optional token to cancel the connection process.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
    /// <remarks>
    /// This function requires the LogPort services to be registered beforehand. Use this in conjunction with <see cref="AddLogPort"/>.
    /// </remarks>
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

    /// <summary>
    /// Synchronously initializes and registers shutdown hooks for the <see cref="LogPortClient"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
    /// <remarks>
    /// This function requires the LogPort services to be registered beforehand. Use this in conjunction with <see cref="AddLogPort"/>.
    /// </remarks>
    public static WebApplication UseLogPort(this WebApplication app)
    {
        return UseLogPortAsync(app).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Adds a LogPort-based logging provider to the ASP.NET Core logging system.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> used to configure logging providers.</param>
    /// <param name="client">The <see cref="LogPortClient"/> instance used to send log entries.</param>
    /// <returns>The same <see cref="ILoggingBuilder"/> instance for chaining.</returns>
    private static ILoggingBuilder AddLogPort(this ILoggingBuilder builder, LogPortClient client)
    {
        builder.AddProvider(new LogPortLoggerProvider(client));
        return builder;
    }
}
