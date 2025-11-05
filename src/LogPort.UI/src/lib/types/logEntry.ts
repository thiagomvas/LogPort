export interface LogEntry {
    message: string;
    level: string;
    timestamp: Date;
    serviceName?: string;
    metadata?: Record<string, any>;
    traceId?: string;
    spanId?: string;
    hostname?: string;
    environment?: string;
}