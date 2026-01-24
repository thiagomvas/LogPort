import { baseFetch } from "../api";
import type { LogPortConfig } from "../types/config";

export function getConfig(): Promise<LogPortConfig> {
  return baseFetch<LogPortConfig>('/api/config/');
}

export async function saveConfig(config: LogPortConfig): Promise<void> {
  await baseFetch<void>('/api/config/', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(config),
  });
}