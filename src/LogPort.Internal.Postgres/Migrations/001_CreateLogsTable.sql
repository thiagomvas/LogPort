CREATE TABLE IF NOT EXISTS logs (
    id BIGSERIAL PRIMARY KEY,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    service_name TEXT,
    level TEXT,
    message TEXT,
    metadata JSONB,
    trace_id TEXT,
    span_id TEXT,
    hostname TEXT,
    environment TEXT
);
