using System.Text.Json.Serialization;

namespace LogPort.Internal.Configuration;

public class PostgresConfig
{
    /// <summary>
    /// Gets or sets whether or not it should use postgres.
    /// </summary>
    /// <remarks>
    /// Currently it is the only data store provider supported, has to be true.
    /// </remarks>
    public bool Use { get; set; } = true;

    /// <summary>
    /// Gets or sets the host of the PostgreSQL database
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the port of the PostgreSQL database
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// Gets or sets the PostgreSQL database
    /// </summary>
    public string Database { get; set; } = "logport";

    /// <summary>
    /// Gets or sets the user of the PostgreSQL database
    /// </summary>
    public string Username { get; set; } = "postgres";

    /// <summary>
    /// Gets or sets the password of the PostgreSQL database
    /// </summary>
    public string Password { get; set; } = "postgres";

    [JsonIgnore]
    public string ConnectionString =>
        $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";

    /// <summary>
    /// Gets or sets the partition length in days of the logs table
    /// </summary>
    public int PartitionLength { get; set; } = 1;
}