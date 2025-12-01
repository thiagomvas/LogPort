using System.Text.Json;
using LogPort.Agent.Endpoints;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace LogPort.Agent;

public static class ApplicationBuilderExtensions
{
    public static void MapAgentEndpoints(this WebApplication app)
    {
        app.MapLogEndpoints();
        app.MapAnalyticsEndpoints();
        
    }
}
