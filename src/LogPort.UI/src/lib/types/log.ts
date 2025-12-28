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

export interface LogMetadata {
  logLevels: string[]
  environments: string[]
  services: string[]
  hostnames: string[]
  logCount: number
  logCountByLevel: Record<string, number>
  logCountByService: Record<string, number>
  logCountByEnvironment: Record<string, number>
  logCountByHostname: Record<string, number>
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
    interval?: string;
  }

export function toQueryString(params: LogQueryParameters): string {
  const query = new URLSearchParams();
  
  if (params.from) {query.append('from', params.from.toISOString());}
  if (params.to) {query.append('to', params.to.toISOString());}
  if (params.level) {query.append('level', params.level);}
  if (params.search) {query.append('search', params.search);}
  if (params.serviceName) {query.append('serviceName', params.serviceName);}
  if (params.hostname) {query.append('hostname', params.hostname);}
  if (params.environment) {query.append('environment', params.environment);}
  if (params.metadata) {query.append('metadata', params.metadata);}
  if (params.traceId) {query.append('traceId', params.traceId);}
  if (params.spanId) {query.append('spanId', params.spanId);}
  if (params.page !== undefined) {query.append('page', params.page.toString());}
  if (params.searchExact !== undefined) {query.append('searchExact', params.searchExact.toString());}
  if (params.pageSize !== undefined) {query.append('pageSize', params.pageSize.toString());}
  if (params.interval) {query.append('interval', params.interval);}
  
  return query.toString();
}

export const placeholderLogs: LogEntry[] = [
  {
    message: "User login successful",
    level: "INFO",
    timestamp: new Date(Date.now() - 1000 * 60 * 5), // 5 minutes ago
    serviceName: "AuthService",
    metadata: { userId: "12345" },
    traceId: "abc123",
    spanId: "span1",
    hostname: "server-01",
    environment: "development",
  },
  {
    message: "Failed to fetch user data",
    level: "ERROR",
    timestamp: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
    serviceName: "UserService",
    metadata: { userId: "67890", error: "NotFound" },
    traceId: "def456",
    spanId: "span2",
    hostname: "server-02",
    environment: "staging",
  },
  {
    message: "Payment processed successfully",
    level: "INFO",
    timestamp: new Date(Date.now() - 1000 * 60 * 60), // 1 hour ago
    serviceName: "PaymentService",
    metadata: { orderId: "98765", amount: 49.99 },
    traceId: "ghi789",
    spanId: "span3",
    hostname: "server-03",
    environment: "production",
  },
  {
    message: "Cache miss for key user_12345",
    level: "WARN",
    timestamp: new Date(Date.now() - 1000 * 60 * 90), // 1.5 hours ago
    serviceName: "CacheService",
    metadata: { key: "user_12345" },
    traceId: "jkl012",
    spanId: "span4",
    hostname: "server-01",
    environment: "development",
  },
  {
    message: "Email sent to user",
    level: "INFO",
    timestamp: new Date(Date.now() - 1000 * 60 * 120), // 2 hours ago
    serviceName: "NotificationService",
    metadata: { email: "user@example.com" },
    traceId: "mno345",
    spanId: "span5",
    hostname: "server-02",
    environment: "staging",
  },
];
  