import { baseFetch } from "../api";
import { toQueryString, type LogEntry, type LogQueryParameters } from "../types/log";

export async function getLogs(params: LogQueryParameters): Promise<LogEntry[]> {
    const queryString = toQueryString(params);
    const result = await baseFetch<any[]>(`/api/logs?${queryString}`);
  
    return result.map(normalizeLog);
  }
  
  export function normalizeLog(log: Record<string, any>): LogEntry {
    return {
      timestamp: log.Timestamp ? new Date(log.Timestamp)
                 : log.timestamp ? new Date(log.timestamp)
                 : undefined,
      message: log.Message ?? log.message,
      level: log.Level ?? log.level,
      serviceName: log.ServiceName ?? log.serviceName,
      hostname: log.Hostname ?? log.hostname,
      environment: log.Environment ?? log.environment,
      metadata: log.Metadata ?? log.metadata,
      traceId: log.TraceId ?? log.traceId,
      spanId: log.SpanId ?? log.spanId,
    };
  }