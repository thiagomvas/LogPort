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
  defaultExpiration: string; 
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
  extractionMode: 'json' | 'regex';
  serviceName: string;
}

export type JsonLogEntryExtractorConfig = BaseLogEntryExtractorConfig & {
  extractionMode: 'json';
  messageProperty: string;
  levelProperty: string;
  timestampProperty: string;
};

export type RegexLogEntryExtractorConfig = BaseLogEntryExtractorConfig & {
  extractionMode: 'regex';
  pattern: string;
  messageGroup: string;
  levelGroup: string;
  timestampGroup: string;
};

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

  extractors: (JsonLogEntryExtractorConfig | RegexLogEntryExtractorConfig)[];
  fileTails: FileTailingConfiguration[];

  mode: LogMode;

  jwtSecret: string;
  jwtIssuer: string;
}

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
    defaultExpiration: "00:10:00"
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
