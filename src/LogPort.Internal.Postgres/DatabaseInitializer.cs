using System.Reflection;

using Npgsql;

namespace LogPort.Data.Postgres;

public sealed class DatabaseInitializer
{
    private readonly Action<string>? _log;

    public DatabaseInitializer(Action<string>? log = null)
    {
        _log = log;
    }

    /// <summary>
    /// Initializes the database by executing embedded SQL scripts in order.
    /// Can optionally drop and recreate tables from scratch.
    /// </summary>
    public async Task InitializeAsync(
        string connectionString,
        bool recreate = false,
        CancellationToken cancellation = default)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var scripts = assembly.GetManifestResourceNames()
            .Where(x => x.EndsWith(".sql"))
            .OrderBy(x => x);

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellation);

        _log?.Invoke("Database connection opened");

        if (recreate)
        {
            cancellation.ThrowIfCancellationRequested();

            _log?.Invoke("Recreating database tables");

            var dropSql = """
                          DROP TABLE IF EXISTS logs;
                          """;

            await using var dropCmd = new NpgsqlCommand(dropSql, conn);
            await dropCmd.ExecuteNonQueryAsync(cancellation);
        }

        foreach (var scriptName in scripts)
        {
            cancellation.ThrowIfCancellationRequested();

            _log?.Invoke($"Executing script: {scriptName}");

            await using var stream = assembly.GetManifestResourceStream(scriptName);
            if (stream == null)
            {
                _log?.Invoke($"Skipped missing resource: {scriptName}");
                continue;
            }

            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync(cancellation);

            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(cancellation);
        }

        _log?.Invoke("Database initialization complete");
    }
}