import { baseFetch } from "../api";
import type { JobMetadata } from "../types/jobs";

export function getJobMetadata(): Promise<JobMetadata[]> {
  return baseFetch<JobMetadata[]>('/api/jobs/');
}