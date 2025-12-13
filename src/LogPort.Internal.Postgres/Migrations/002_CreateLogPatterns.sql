BEGIN;

CREATE TABLE IF NOT EXISTS log_patterns (
    id BIGSERIAL PRIMARY KEY,
    normalized_message TEXT NOT NULL,
    pattern_hash TEXT NOT NULL,
    first_seen TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_seen  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    occurrence_count BIGINT NOT NULL DEFAULT 0,
    CONSTRAINT uq_log_patterns_hash UNIQUE (pattern_hash)
);

ALTER TABLE logs
    ADD COLUMN IF NOT EXISTS pattern_id BIGINT;

ALTER TABLE logs
    ADD CONSTRAINT fk_logs_pattern
        FOREIGN KEY (pattern_id)
            REFERENCES log_patterns(id)
    NOT VALID;

CREATE INDEX IF NOT EXISTS idx_logs_pattern_id
    ON logs (pattern_id);

CREATE INDEX IF NOT EXISTS idx_log_patterns_hash
    ON log_patterns (pattern_hash);

CREATE INDEX IF NOT EXISTS idx_log_patterns_normalized_message
    ON log_patterns USING gin (normalized_message gin_trgm_ops);

COMMIT;
