import { baseFetch } from "../api";
import { toQueryString, type LogEntry, type LogMetadata, type LogQueryParameters } from "../types/log";

export async function getLogs(params: LogQueryParameters): Promise<LogEntry[]> {
  const queryString = toQueryString(params);
  const result = await baseFetch<any[]>(`/api/logs?${queryString}`);
  
  return result.map(normalizeLog);
}

export async function queryLogs(query: string, from?: Date, to?: Date, page: number = 1, pageSize: number = 100): Promise<LogEntry[]> {
  const qs = new URLSearchParams();
  qs.set('query', query);
  qs.set('page', page.toString());
  qs.set('pageSize', pageSize.toString());
  
  if (from) {
    qs.set('from', from.toISOString());
  }

  if (to) {
    qs.set('to', to.toISOString());
  }
  
  const result = await baseFetch<any[]>(`/api/logs/query?${qs.toString()}`);
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

export function getMetadata(
  params?: {
    from?: Date | string;
    to?: Date | string;
    lastDays?: number;
  }
): Promise<LogMetadata> {
  const qs = new URLSearchParams();

  if (params?.from) {
    qs.set(
      'from',
      params.from instanceof Date
        ? params.from.toISOString()
        : params.from
    );
  }

  if (params?.to) {
    qs.set(
      'to',
      params.to instanceof Date
        ? params.to.toISOString()
        : params.to
    );
  }

  if (params?.lastDays) {
    qs.set('lastDays', String(params!.lastDays));
  }

  const url = qs.toString()
    ? `/api/logs/metadata?${qs.toString()}`
    : '/api/logs/metadata';

  return baseFetch<LogMetadata>(url);
}
