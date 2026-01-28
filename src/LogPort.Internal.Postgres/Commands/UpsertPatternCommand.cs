using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public static class UpsertPatternCommand
{
    public static SqlCommand Create(
        string normalizedMessage,
        ulong patternHash,
        DateTime timestamp,
        string level)
    {
        const string sql = @"
INSERT INTO log_patterns
    (normalized_message, pattern_hash, first_seen, last_seen, occurrence_count, level)
VALUES
    (@msg, @hash, @ts, @ts, 1, @lvl)
ON CONFLICT (pattern_hash)
DO UPDATE SET
    last_seen = EXCLUDED.last_seen,
    occurrence_count = log_patterns.occurrence_count + 1
RETURNING id;
";

        return new SqlCommand(sql, new
        {
            msg = normalizedMessage,
            hash = unchecked((long)patternHash),
            ts = timestamp,
            lvl = level
        });
    }
}

