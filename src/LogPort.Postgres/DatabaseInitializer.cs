using System.Reflection;
using Npgsql;

namespace LogPort.Postgres;

public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes the database by executing embedded SQL scripts in order.
    /// Can optionally drop and recreate tables from scratch.
    /// </summary>
    /// <param name="connectionString">Postgres connection string</param>
    /// <param name="recreate">If true, drop tables first</param>
    public static async Task InitializeAsync(string connectionString, bool recreate = false)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var scripts = assembly.GetManifestResourceNames()
            .Where(x => x.EndsWith(".sql"))
            .OrderBy(x => x);

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        if (recreate)
        {
            // Drop known tables (adjust as needed)
            var dropSql = @"
                DROP TABLE IF EXISTS logs;
            ";

            await using var dropCmd = new NpgsqlCommand(dropSql, conn);
            await dropCmd.ExecuteNonQueryAsync();

            Console.WriteLine("Existing tables dropped.");
        }

        foreach (var scriptName in scripts)
        {
            await using var stream = assembly.GetManifestResourceStream(scriptName);
            if (stream == null) continue;

            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine("Database initialization complete.");
    }
}