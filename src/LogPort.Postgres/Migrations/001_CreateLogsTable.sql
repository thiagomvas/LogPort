CREATE TABLE IF NOT EXISTS logs (
    id BIGSERIAL PRIMARY KEY,
    timestamp TIMESTAMPTZ NOT NULL,
    service_name TEXT NOT NULL,
    level TEXT NOT NULL,
    message TEXT NOT NULL,
    metadata JSONB,
    trace_id TEXT,
    span_id TEXT,
    hostname TEXT,
    environment TEXT
);
