export interface LogEntry {
    message?: string;
    level?: string;
    timestamp?: Date;
    serviceName?: string;
    metadata?: Record<string, any>;
    traceId?: string;
    spanId?: string;
    hostname?: string;
    environment?: string;
}

export interface LogQueryParameters {
    from?: Date;
    to?: Date;
    level?: string;
    search?: string;
    serviceName?: string;
    hostname?: string;
    environment?: string;
    metadata?: string;
    traceId?: string;
    spanId?: string;
    page?: number;
    searchExact?: boolean;
    pageSize?: number;
  }

  export function toQueryString(params: LogQueryParameters): string {
    const query = new URLSearchParams();
  
    if (params.from) query.append('from', params.from.toISOString());
    if (params.to) query.append('to', params.to.toISOString());
    if (params.level) query.append('level', params.level);
    if (params.search) query.append('search', params.search);
    if (params.serviceName) query.append('serviceName', params.serviceName);
    if (params.hostname) query.append('hostname', params.hostname);
    if (params.environment) query.append('environment', params.environment);
    if (params.metadata) query.append('metadata', params.metadata);
    if (params.traceId) query.append('traceId', params.traceId);
    if (params.spanId) query.append('spanId', params.spanId);
    if (params.page !== undefined) query.append('page', params.page.toString());
    if (params.searchExact !== undefined) query.append('searchExact', params.searchExact.toString());
    if (params.pageSize !== undefined) query.append('pageSize', params.pageSize.toString());
  
    return query.toString();
  }