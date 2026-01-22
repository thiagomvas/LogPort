import { baseFetch } from "../api";
import type { JobMetadata } from "../types/jobs";

export function getJobMetadata(): Promise<JobMetadata[]> {
  return baseFetch<JobMetadata[]>('/api/jobs/');
}

export async function triggerJob(id: string): Promise<void> {
  await baseFetch<void>(`/api/jobs/${id}/trigger`, {
    method: 'POST',
  });
}
