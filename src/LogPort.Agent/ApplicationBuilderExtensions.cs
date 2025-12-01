using LogPort.Agent.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace LogPort.Agent;

public static class ApplicationBuilderExtensions
{
    public static void MapAgentEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    status = report.Status.ToString(),
                    totalChecks = report.Entries.Count,
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        exception = e.Value.Exception?.Message,
                        duration = e.Value.Duration.ToString()
                    })
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        });

        app.MapLogEndpoints();
        app.MapAnalyticsEndpoints();
    }
}
