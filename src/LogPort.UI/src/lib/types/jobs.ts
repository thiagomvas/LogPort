export interface Job {
    id: string;
    cron: string;
    lastExecution: Date;
    nextExecution: Date;
}