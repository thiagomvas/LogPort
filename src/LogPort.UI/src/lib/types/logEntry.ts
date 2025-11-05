export interface LogEntry {
    Message?: string;
    Level?: string;
    Timestamp?: Date;
    ServiceName?: string;
    Metadata?: Record<string, any>;
    TraceId?: string;
    SpanId?: string;
    Hostname?: string;
    Environment?: string;
}