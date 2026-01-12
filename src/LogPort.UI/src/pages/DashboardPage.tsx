import { useEffect, useState } from 'react';
import '../styles/dashboardPage.css';
import { getMetadata } from '../lib/services/logs.service';
import type { LogMetadata } from '../lib/types/log';
import type { TimeRange } from '../lib/types/timeRange';
import type { MetricSnapshot } from '../lib/types/metrics';
import { fetchLiveMetrics } from '../lib/services/metrics.service';
import TimeRangeDropdown from '../components/timeRangeDropdown';
import MetricCard from '../components/metricCard';

function DashboardPage() {
  const [meta, setMeta] = useState<LogMetadata | null>(null);
  const [range, setRange] = useState<TimeRange | null>(null);
  const [liveMetrics, setLiveMetrics] = useState<MetricSnapshot | null>(null);

  // --- Fetch log metadata ---
  useEffect(() => {
    const to = range?.to ?? new Date();
    const from =
      range?.from ??
      (() => {
        const d = new Date(to);
        d.setHours(d.getHours() - 24);
        return d;
      })();

    getMetadata({ from, to })
      .then(setMeta)
      .catch(console.error);
  }, [range]);

  useEffect(() => {
    const interval = setInterval(() => {
      fetchLiveMetrics()
        .then(metrics => {
          if (metrics) {setLiveMetrics(metrics);}
        })
        .catch(err => console.error('Failed to fetch live metrics', err));
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  if (!meta || !liveMetrics) {return <div>Loading dashboard…</div>;}

  const total = meta.logCount;
  const errors = meta.logCountByLevel?.Error ?? 0;
  const topService = topEntry(meta.logCountByService);
  const topEnv = topEntry(meta.logCountByEnvironment);
  const topHost = topEntry(meta.logCountByHostname);

  return (
    <div className="dashboard-page">
      <TimeRangeDropdown onChange={setRange} />

      <div className="stat-grid">
        <Stat title="Total Logs" value={total} />
        <Stat title="Error Rate" value={`${percentage(errors, total)}%`} intent="danger" />
        <Stat title="Top Service" value={topService?.[0] ?? '—'} />
        <Stat title="Top Environment" value={topEnv?.[0] ?? '—'} />
      </div>

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

      <section>
        <h3>Live Metrics</h3>
        <div className="metrics-grid">
          {Object.entries(liveMetrics?.counters || {}).map(([name, metric]) => (
            <MetricCard
              key={name}
              name={name}
              last1s={metric.last1s}
              last10s={metric.last10s}
              last1m={metric.last1m}
              sparkline={metric.buckets || []}
              timeRange={name.endsWith('1h') ? '1m' : '24h'}
            />
          ))}
        </div>
      </section>

      {/* --- Actions --- */}
      <div className="dashboard-actions">
        <a href="/logs/tail" className="btn btn-primary">Live Tail</a>
        <a href="/logs" className="btn">Explore Logs</a>
        <a href="/logs" className="btn btn-danger">Errors Only</a>
      </div>
    </div>
  );
}

// ---------------- COMPONENTS ----------------
function Stat({ title, value, intent }: any) {
  return (
    <div className={`stat-card ${intent ?? ''}`}>
      <span className="stat-title">{title}</span>
      <span className="stat-value">{value}</span>
    </div>
  );
}

function KeyValueBars({ data, limit = 10 }: { data: Record<string, number>; limit?: number }) {
  const entries = Object.entries(data).sort((a, b) => b[1] - a[1]).slice(0, limit);
  const max = Math.max(...entries.map(e => e[1]), 1);
  return (
    <div className="kv-bars">
      {entries.map(([key, value]) => (
        <div key={key} className="kv-row">
          <span>{key || '(null)'}</span>
          <div className="bar">
            <div className="fill" style={{ width: `${(value / max) * 100}%` }} />
          </div>
          <span>{value}</span>
        </div>
      ))}
    </div>
  );
}

// ---------------- HELPERS ----------------
const topEntry = (map: Record<string, number>) => Object.entries(map).sort((a, b) => b[1] - a[1])[0];
const percentage = (part: number, total: number) => (total === 0 ? 0 : Math.round((part / total) * 100));

export default DashboardPage;
