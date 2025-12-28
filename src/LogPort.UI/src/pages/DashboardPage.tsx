import { useEffect, useState } from 'react';
import '../styles/dashboardPage.css';
import { getMetadata } from '../lib/services/logs.service';
import type { LogMetadata } from '../lib/types/log';
import type { TimeRange } from '../lib/types/timeRange';
import TimeRangeDropdown from '../components/timeRangeDropdown';

const topEntry = (map: Record<string, number>) =>
  Object.entries(map)
    .sort((a, b) => b[1] - a[1])[0];

const percentage = (part: number, total: number) =>
  total === 0 ? 0 : Math.round((part / total) * 100);


function DashboardPage() {
  const [meta, setMeta] = useState<LogMetadata | null>(null);
  const [range, setRange] = useState<TimeRange | null>(null);
  
  useEffect(() => {
    const to = range?.to ?? new Date();
    const from =
    range?.from ??
    (() => {
      const d = new Date(to);
      d.setHours(d.getHours() - 24);
      return d;
    })();
    console.log("Requerying with", { from, to });
    getMetadata({ from, to })
      .then(setMeta)
      .catch(console.error);
    console.log("Got results ", meta);
  }, [range]);


  if (!meta) {return <div>Loading dashboard…</div>;}

  const total = meta.logCount;
  const errors = meta.logCountByLevel?.error ?? 0;

  const topService = topEntry(meta.logCountByService);
  const topEnv = topEntry(meta.logCountByEnvironment);
  const topHost = topEntry(meta.logCountByHostname);

  return (
    <div className="dashboard-page">
      <TimeRangeDropdown onChange={setRange} />
      {/* Summary */}
      <div className="stat-grid">
        <Stat title="Total Logs" value={total} />
        <Stat
          title="Error Rate"
          value={`${percentage(errors, total)}%`}
          intent="danger"
        />
        <Stat title="Top Service" value={topService?.[0] ?? '—'} />
        <Stat title="Top Environment" value={topEnv?.[0] ?? '—'} />
      </div>

      {/* Distributions */}
      <section>
        <h3>Logs by Level</h3>
        <KeyValueBars data={meta.logCountByLevel} />
      </section>

      <section>
        <h3>Top Services</h3>
        <KeyValueBars data={meta.logCountByService} limit={5} />
      </section>

      <section>
        <h3>Top Hosts</h3>
        <KeyValueBars data={meta.logCountByHostname} limit={5} />
      </section>

      {/* Actions */}
      <div className="dashboard-actions">
        <a href="/logs/tail" className="btn btn-primary">
          Live Tail
        </a>
        <a href="/logs/explorer" className="btn">
          Explore Logs
        </a>
        <a href="/logs/explorer?level=error" className="btn btn-danger">
          Errors Only
        </a>
      </div>
    </div>
  );
}

function Stat({ title, value, intent }: any) {
  return (
    <div className={`stat-card ${intent ?? ''}`}>
      <span className="stat-title">{title}</span>
      <span className="stat-value">{value}</span>
    </div>
  );
}

function KeyValueBars({
  data,
  limit = 10,
}: {
  data: Record<string, number>;
  limit?: number;
}) {
  const entries = Object.entries(data)
    .sort((a, b) => b[1] - a[1])
    .slice(0, limit);

  const max = Math.max(...entries.map(e => e[1]), 1);

  return (
    <div className="kv-bars">
      {entries.map(([key, value]) => (
        <div key={key} className="kv-row">
          <span>{key || '(null)'}</span>
          <div className="bar">
            <div
              className="fill"
              style={{ width: `${(value / max) * 100}%` }}
            />
          </div>
          <span>{value}</span>
        </div>
      ))}
    </div>
  );
}


export default DashboardPage;
