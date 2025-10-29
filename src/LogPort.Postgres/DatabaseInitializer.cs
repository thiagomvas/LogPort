using System.Reflection;
using Npgsql;

namespace LogPort.Postgres;

public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes the database by executing embedded SQL scripts in order.
    /// </summary>
    /// <param name="connectionString">Postgres connection string</param>
    public static async Task InitializeAsync(string connectionString)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var scripts = assembly.GetManifestResourceNames()
            .Where(x => x.EndsWith(".sql"))
            .OrderBy(x => x); 

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

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