import { baseFetch } from "../api";
import type { Job } from "../types/jobs";

export function getRecurringJobs(): Promise<Job[]> {
  return baseFetch<Job[]>('/api/jobs/recurring');
}