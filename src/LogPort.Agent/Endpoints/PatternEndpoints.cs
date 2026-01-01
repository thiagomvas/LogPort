using LogPort.Core.Models;
using LogPort.Internal.Services;

namespace LogPort.Agent.Endpoints;

public static class PatternEndpoints
{
    public static void MapPatternEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/patterns")
            .WithTags("Patterns");

        group.MapGet("/", GetPatterns)
            .WithName("GetPatterns")
            .WithSummary("Retrieve log patterns")
            .WithDescription("Retrieves a list of log patterns stored in the system.")
            .Produces<List<LogPattern>>(StatusCodes.Status200OK);
    }
    private static async Task<IResult> GetPatterns(
        LogService logService,
        int limit = 100,
        int offset = 0)
    {
        var patterns = await logService.GetLogPatternsAsync(limit, offset);
        return Results.Ok(patterns);
    }
}