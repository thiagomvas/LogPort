import { baseFetch } from "../api";
import type { MetricSnapshot } from "../types/metrics";

export async function fetchLiveMetrics(): Promise<MetricSnapshot> {
  return baseFetch<MetricSnapshot>('/api/metrics');
}