export interface JobMetadata {
    id: string;
    name: string;
    description: string;
    lastExecution: string | null;
    nextExecution: string | null;
    isEnabled: boolean;
    cron: string;
}
