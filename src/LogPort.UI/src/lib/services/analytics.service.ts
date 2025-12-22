import { baseFetch } from "../api";
import type { LogBucket } from "../types/analytics";
import { type LogQueryParameters, toQueryString } from "../types/log";

export async function getHistogramData(params: LogQueryParameters): Promise<LogBucket[]> {
    const queryString = toQueryString(params);
    const result = await baseFetch<{ periodStart: string; count: number }[]>(`/api/analytics/histogram?${queryString}`);
  
    return result.map(bucket => ({
      periodStart: bucket.periodStart ? new Date(bucket.periodStart) : new Date(0),
      count: bucket.count ?? 0,
    }));
  }
  
