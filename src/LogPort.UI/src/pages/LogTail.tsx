import { useEffect, useRef, useState } from 'react';
import '../styles/logsPage.css';
import { HistogramChart } from '../components/histogram';
import { LogViewer } from '../components/logViewer';
import { getLogs, normalizeLog } from '../lib/services/logs.service';
import type { LogBucket } from '../lib/types/analytics';
import type { LogEntry, LogQueryParameters } from '../lib/types/log';

const FIFTEEN_MINUTES_MS = 15 * 60 * 1000;
const ONE_MINUTE_MS = 60 * 1000;

function LogTailPage() {
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [histogram, setHistogram] = useState<LogBucket[]>([]);
  const wsRef = useRef<WebSocket | null>(null);

  useEffect(() => {
    const cutoff = Date.now() - FIFTEEN_MINUTES_MS;

    const buildHistogram = (entries: LogEntry[]) => {
      const buckets = new Map<number, number>();

      for (const log of entries) {
        if (!log.timestamp) {continue;}
        const ts = new Date(log.timestamp).getTime();
        if (ts < cutoff) {continue;}

        const bucketTs =
                    Math.floor(ts / ONE_MINUTE_MS) * ONE_MINUTE_MS;
        buckets.set(bucketTs, (buckets.get(bucketTs) ?? 0) + 1);
      }

      return Array.from(buckets.entries())
        .sort(([a], [b]) => a - b)
        .map(([ts, count]) => ({
          periodStart: new Date(ts),
          count,
        }));
    };

    const loadRecentLogsAndTail = async () => {
      try {
        // 1️⃣ Load recent logs (last 15 minutes)
        const params: LogQueryParameters = {
          page: 1,
          pageSize: 500,
        };

        const fetched = await getLogs(params);
        const normalized = fetched.map(normalizeLog);

        const recentLogs = normalized.filter(l => {
          const ts = l.timestamp
            ? new Date(l.timestamp).getTime()
            : 0;
          return ts >= cutoff;
        });

        setLogs(recentLogs);
        setHistogram(buildHistogram(recentLogs));
      } catch (err) {
        console.error('Failed to load recent logs', err);
      }

      // 2️⃣ Start live tailing
      const wsHost =
        import.meta.env.LOGPORT_AGENT_URL || window.location.host;

      const wsProtocol = window.location.protocol === "https:" ? "wss" : "ws";
      const wsUrl = `${wsProtocol}://${wsHost}/api/live-logs`;

      const ws = new WebSocket(wsUrl);
      wsRef.current = ws;

      ws.onmessage = (event) => {
        try {
          const rawLogs: any[] = JSON.parse(event.data);
          const incomingLogs: LogEntry[] =
                        rawLogs.map(normalizeLog);

          const cutoff = Date.now() - FIFTEEN_MINUTES_MS;

          setLogs(prev => {
            const merged = [...incomingLogs.reverse(), ...prev];
            return merged.filter(l => {
              const ts = l.timestamp
                ? new Date(l.timestamp).getTime()
                : 0;
              return ts >= cutoff;
            });
          });

          setHistogram(prev => {
            const buckets = new Map<number, number>();

            // Keep existing buckets within window
            for (const b of prev) {
              const ts = b.periodStart.getTime();
              if (ts >= cutoff) {
                buckets.set(ts, b.count);
              }
            }

            // Add incoming logs
            for (const log of incomingLogs) {
              if (!log.timestamp) {continue;}
              const ts = new Date(log.timestamp).getTime();
              if (ts < cutoff) {continue;}

              const bucketTs =
                                Math.floor(ts / ONE_MINUTE_MS) *
                                ONE_MINUTE_MS;
              buckets.set(
                bucketTs,
                (buckets.get(bucketTs) ?? 0) + 1
              );
            }

            return Array.from(buckets.entries())
              .sort(([a], [b]) => a - b)
              .map(([ts, count]) => ({
                periodStart: new Date(ts),
                count,
              }));
          });
        } catch (err) {
          console.error('Failed to parse live logs', err);
        }
      };

      ws.onerror = err => console.error('WebSocket error', err);
      ws.onclose = () =>
        console.log('Live log tailing disconnected');
    };

    loadRecentLogsAndTail();

    return () => {
      wsRef.current?.close();
    };
  }, []);

  return (
    <div className="logs-page">
      <div className="controls-bar">
        <span className="btn btn-active">
                    Live Tailing (Last 15 Minutes)
        </span>            </div>

      <div className="log-container">
        <HistogramChart data={histogram} timeUnit="minute" />
        <LogViewer logs={logs} />
      </div>
    </div>
  );
}

export default LogTailPage;
