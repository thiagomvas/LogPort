// types/config.ts

export type LogMode = 'Agent' | 'Relay';

export interface MetricsConfig {
  bucketDuration: number; // ms
  maxWindow: number; // ms
}

export interface PostgresConfig {
  use: boolean;
  host: string;
  port: number;
  database: string;
  username: string;
  password: string;
  partitionLength: number;
}

export interface DockerConfig {
  use: boolean;
  socketPath: string;
  watchAllContainers: boolean;
}

export interface CacheConfig {
  useRedis: boolean;
  redisConnectionString?: string;
  defaultExpiration: number; // ms
}

export interface LogRetentionConfig {
  automaticCleanupCron: string;
  retentionDays: number;
  enableAutomaticCleanupJob: boolean;
}

export interface LevelRetentionConfig {
  automaticCleanupCron: string;
  retentions: Record<string, number>; // ms per log level
  enableAutomaticCleanupJob: boolean;
}

export interface FileTailingConfiguration {
  serviceName: string;
  path: string;
}

export interface BaseLogEntryExtractorConfig {
  type: string;
  options?: Record<string, any>;
}

export interface LogPortConfig {
  postgres: PostgresConfig;
  docker: DockerConfig;
  cache: CacheConfig;
  metrics: MetricsConfig;
  retention: LogRetentionConfig;
  levelRetention: LevelRetentionConfig;

  port: number;
  upstreamUrl?: string;

  batchSize: number;
  flushIntervalMs: number;

  adminLogin: string;
  adminPassword: string;
  apiSecret: string;

  extractors: BaseLogEntryExtractorConfig[];
  fileTails: FileTailingConfiguration[];

  mode: LogMode;

  jwtSecret: string;
  jwtIssuer: string;
}

// Default config helper
export const defaultLogPortConfig: LogPortConfig = {
  postgres: {
    use: true,
    host: 'localhost',
    port: 5432,
    database: 'logport',
    username: 'postgres',
    password: 'postgres',
    partitionLength: 1,
  },
  docker: {
    use: false,
    socketPath: 'unix:///var/run/docker.sock',
    watchAllContainers: false,
  },
  cache: {
    useRedis: false,
    redisConnectionString: undefined,
    defaultExpiration: 10 * 60 * 1000, // 10 minutes
  },
  metrics: {
    bucketDuration: 10 * 1000, // 10s
    maxWindow: 15 * 60 * 1000, // 15 min
  },
  retention: {
    automaticCleanupCron: '0 3 * * *',
    retentionDays: 14,
    enableAutomaticCleanupJob: true,
  },
  levelRetention: {
    automaticCleanupCron: '0 3 * * *',
    retentions: {},
    enableAutomaticCleanupJob: true,
  },
  port: 8080,
  upstreamUrl: undefined,
  batchSize: 100,
  flushIntervalMs: 250,
  adminLogin: 'admin',
  adminPassword: 'changeme',
  apiSecret: crypto.randomUUID().replace(/-/g, ''),
  extractors: [],
  fileTails: [],
  mode: 'Agent',
  jwtSecret: '',
  jwtIssuer: '',
};
