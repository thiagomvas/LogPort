using LogPort.Core;
using LogPort.Core.Models;
using LogPort.SDK;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    public static IHostApplicationBuilder AddLogPort(this IHostApplicationBuilder builder, Action<LogPortClientConfig>? configure = null)
    {
        var config = LogPortClientConfig.LoadFromEnvironment();
        configure?.Invoke(config);
        builder.Services.AddSingleton(config);

        var normalizer = new LogNormalizer();

        builder.Services.AddSingleton<ILogPortLogger>(sp =>
        {
            var msLogger = sp.GetRequiredService<ILogger<LogPortClient>>();
            return new MicrosoftLoggerAdapter<LogPortClient>(msLogger);
        });

        builder.Services.AddSingleton<LogPortClient>(sp =>
        {
            var logPortLogger = sp.GetRequiredService<ILogPortLogger>();
            return new LogPortClient(config, normalizer, logger: logPortLogger);
        });
        builder.Services.AddSingleton(normalizer);
        builder.Services.AddSingleton<ILoggerProvider>(sp =>
            new LogPortLoggerProvider(sp, config));


        return builder;
    }

    /// <summary>
    /// Initializes LogPort and starts the background connection asynchronously.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The same application builder for chaining.</returns>
    /// <remarks>
    /// This method starts the LogPort client in a non-blocking manner, registers
    /// HTTP request logging middleware, and ensures logs are flushed on shutdown.
    /// </remarks>
    public static async Task<IApplicationBuilder> UseLogPortAsync(this IApplicationBuilder app, CancellationToken cancellationToken = default)
    {
        var client = app.ApplicationServices.GetRequiredService<LogPortClient>();
        _ = Task.Run(() => client.EnsureConnectedAsync(cancellationToken), cancellationToken);

        app.UseMiddleware<LogPortAspNetMiddleware>();

        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() =>
        {
            client.FlushAsync().GetAwaiter().GetResult();
            client.Dispose();
        });

        return app;
    }

    /// <summary>
    /// Initializes LogPort and starts the background connection synchronously.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder for chaining.</returns>
    /// <remarks>
    /// This is a synchronous wrapper around <see cref="UseLogPortAsync"/> and is not recommended.
    /// </remarks>
    public static IApplicationBuilder UseLogPort(this IApplicationBuilder app)
    {
        return app.UseLogPortAsync().GetAwaiter().GetResult();
    }
}