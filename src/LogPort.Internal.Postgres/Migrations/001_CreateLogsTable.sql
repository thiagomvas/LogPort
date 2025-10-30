CREATE TABLE IF NOT EXISTS logs (
    id BIGSERIAL NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    service_name TEXT,
    level TEXT,
    message TEXT,
    metadata JSONB,
    trace_id TEXT,
    span_id TEXT,
    hostname TEXT,
    environment TEXT,
    PRIMARY KEY (id, timestamp)  
) PARTITION BY RANGE (timestamp);
