import { useEffect, useState } from 'react';
import '../styles/jobsPage.css';
import Stat from '../components/stat';
import { formatDateTime, timeAgo } from '../lib/utils/date';
import { getJobMetadata } from '../lib/services/jobs.service';
import type { JobMetadata } from '../lib/types/jobs';

function JobsPage() {
  const [jobs, setJobs] = useState<JobMetadata[]>([]);

  useEffect(() => {
    getJobMetadata().then(setJobs).catch(console.error);
  }, []);

  const now = new Date();

  const enabledJobs = jobs.filter(j => j.isEnabled);
  const disabledJobs = jobs.filter(j => !j.isEnabled);

  const overdueJobs = enabledJobs.filter(
    j => j.nextExecution && new Date(j.nextExecution) < now
  );

  const activeJobs = enabledJobs.filter(j => j.nextExecution);

  return (
    <div className="jobs-page">
      <div className="stat-grid">
        <Stat title="Total Jobs" value={jobs.length} />
        <Stat title="Active Jobs" value={activeJobs.length} />
        <Stat title="Disabled Jobs" value={disabledJobs.length} />
        <Stat
          title="Overdue Jobs"
          value={overdueJobs.length}
          intent={overdueJobs.length > 0 ? 'danger' : undefined}
        />
      </div>

      <section>
        <h3>Recurring Jobs</h3>

        <div className="jobs-table">
          <div className="jobs-header">
            <span>Job</span>
            <span>Cron</span>
            <span>Last Run</span>
            <span>Next Run</span>
            <span>Status</span>
          </div>

          {jobs.map(job => {
            const next = job.nextExecution
              ? new Date(job.nextExecution)
              : null;

            const isOverdue = !!(
              job.isEnabled &&
              next &&
              next < now
            );

            const status = !job.isEnabled
              ? 'disabled'
              : !job.nextExecution
                ? 'warn'
                : isOverdue
                  ? 'error'
                  : 'ok';

            return (
              <div
                key={job.id}
                className={`jobs-row ${status}`}
                title={job.description}
              >
                <div className="job-main">
                  <p className="job-name">{job.name}</p>
                  <span className="subtitle">
                    {job.description}
                  </span>
                </div>

                <span className="job-cron">{job.cron}</span>

                <span className="job-time">
                  {job.lastExecution
                    ? timeAgo(job.lastExecution)
                    : 'Never'}
                </span>

                <span className="job-time">
                  {next ? formatDateTime(next) : '—'}
                </span>

                <span className={`job-status ${status}`}>
                  {status === 'ok' && 'OK'}
                  {status === 'warn' && 'Never Run'}
                  {status === 'error' && 'Overdue'}
                  {status === 'disabled' && 'Disabled'}
                </span>
              </div>
            );
          })}
        </div>
      </section>
    </div>
  );
}

export default JobsPage;
