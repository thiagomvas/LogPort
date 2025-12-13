BEGIN;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE TABLE IF NOT EXISTS log_patterns (
    id BIGSERIAL PRIMARY KEY,
    normalized_message TEXT NOT NULL,
    pattern_hash BIGINT NOT NULL UNIQUE,
    first_seen TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_seen  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    occurrence_count BIGINT NOT NULL DEFAULT 0,
    CONSTRAINT uq_log_patterns_hash UNIQUE (pattern_hash)
);

ALTER TABLE logs
    ADD COLUMN IF NOT EXISTS pattern_id BIGINT;

CREATE INDEX IF NOT EXISTS idx_logs_pattern_id
    ON logs (pattern_id);

CREATE INDEX IF NOT EXISTS idx_log_patterns_hash
    ON log_patterns (pattern_hash);

CREATE INDEX IF NOT EXISTS idx_log_patterns_normalized_message
    ON log_patterns USING gin (normalized_message gin_trgm_ops);

COMMIT;
