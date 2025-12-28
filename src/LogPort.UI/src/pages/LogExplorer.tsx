import { useState, useEffect, useRef } from 'react';

import '../styles/logsPage.css';
import { HistogramChart } from '../components/histogram';
import { LogViewer } from '../components/logViewer';
import { getHistogramData } from '../lib/services/analytics.service';
import { getMetadata, getLogs } from '../lib/services/logs.service';
import type { LogBucket } from '../lib/types/analytics';
import type { LogEntry, LogMetadata, LogQueryParameters } from '../lib/types/log';

const toInputValue = (d?: Date) =>
  d
    ? new Date(d.getTime() - d.getTimezoneOffset() * 60000)
      .toISOString()
      .slice(0, 16)
    : '';

function LogExplorer() {
  const initialTo = new Date();
  const initialFrom = new Date(initialTo);
  initialFrom.setHours(initialTo.getHours() - 24);

  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [histogram, setHistogram] = useState<LogBucket[]>([]);
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [metadata, setMetadata] = useState<LogMetadata | null>(null);

  const [queryParams, setQueryParams] = useState<LogQueryParameters>({
    from: initialFrom,
    to: initialTo,
    page: 1,
    pageSize: 100,
    level: '',
    search: '',
    serviceName: '',
    hostname: '',
    environment: '',
  });

  const lastUpdatedRef = useRef<Date | null>(null);
  const wsRef = useRef<WebSocket | null>(null);
  const loadingRef = useRef(false);
  const pageRef = useRef(1);
  const queryParamsRef = useRef(queryParams);

  useEffect(() => {
    queryParamsRef.current = queryParams;
  }, [queryParams]);

  useEffect(() => {
    pageRef.current = page;
  }, [page]);

  useEffect(() => {
    fetchLatestLogs();
    getMetadata().then(setMetadata).catch(console.error);
    return () => {
      wsRef.current?.close();
    };
  }, []);

  useEffect(() => {
    setLogs([]);
    setHasMore(true);
    setPage(1);
    pageRef.current = 1;
    fetchLatestLogs();
  }, [queryParams.from, queryParams.to]);

  useEffect(() => {
    const handleScroll = () => {
      if (loadingRef.current || !hasMore) {return;}
      const nearBottom =
        window.innerHeight + window.scrollY >=
        document.body.offsetHeight - 300;
      if (nearBottom) {fetchLogsPage();}
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [hasMore]);

  const fetchLogsPage = async () => {
    if (loadingRef.current) {return;}
    loadingRef.current = true;
    setLoading(true);

    try {
      const params: LogQueryParameters = {
        ...queryParamsRef.current,
        page: pageRef.current,
      };

      const newLogs = await getLogs(params);
      if (!newLogs || newLogs.length === 0) {
        setHasMore(false);
        return;
      }

      setLogs(prev => [...prev, ...newLogs]);
      setPage(prev => prev + 1);

      const latest = newLogs.reduce((max, log) => {
        const ts = log.timestamp ? new Date(log.timestamp) : new Date(0);
        return ts > max ? ts : max;
      }, lastUpdatedRef.current ?? new Date(0));

      lastUpdatedRef.current = latest;

      if (pageRef.current === 1 || pageRef.current % 3 === 0) {
        const histogramData = await getHistogramData({
          from: queryParamsRef.current.from,
          to: queryParamsRef.current.to,
        });
        setHistogram(histogramData);
      }
    } catch (err) {
      console.error(err);
    } finally {
      loadingRef.current = false;
      setLoading(false);
    }
  };

  const fetchLatestLogs = async () => {
    if (loadingRef.current) {return;}
    loadingRef.current = true;
    setLoading(true);

    try {
      const params: LogQueryParameters = {
        ...queryParamsRef.current,
        page: 1,
      };

      const newLogs = await getLogs(params);
      setLogs(newLogs);
      setPage(2);
      pageRef.current = 2;
      setHasMore(true);

      const latest = newLogs.reduce((max, log) => {
        const ts = log.timestamp ? new Date(log.timestamp) : new Date(0);
        return ts > max ? ts : max;
      }, lastUpdatedRef.current ?? new Date(0));

      lastUpdatedRef.current = latest;

      const histogramData = await getHistogramData({
        from: queryParamsRef.current.from,
        to: queryParamsRef.current.to,
      });
      setHistogram(histogramData);
    } catch (err) {
      console.error(err);
    } finally {
      loadingRef.current = false;
      setLoading(false);
    }
  };

  const applyFilters = () => {
    setLogs([]);
    setHasMore(true);
    setPage(1);
    pageRef.current = 1;
    fetchLatestLogs();
  };

  return (
    <div className="logs-page">
      <div className="filter-bar">
        <input
          type="datetime-local"
          value={toInputValue(queryParams.from)}
          onChange={e =>
            setQueryParams(p => ({
              ...p,
              from: e.target.value ? new Date(e.target.value) : undefined,
            }))
          }
        />

        <input
          type="datetime-local"
          value={toInputValue(queryParams.to)}
          onChange={e =>
            setQueryParams(p => ({
              ...p,
              to: e.target.value ? new Date(e.target.value) : undefined,
            }))
          }
        />

        <input
          type="text"
          placeholder="Search logs..."
          value={queryParams.search || ''}
          onChange={e =>
            setQueryParams(p => ({ ...p, search: e.target.value }))
          }
        />

        <select
          value={queryParams.level || ''}
          onChange={e =>
            setQueryParams(p => ({ ...p, level: e.target.value }))
          }
        >
          <option value="">All Levels</option>
          {metadata?.logLevels?.map((lvl, i) => (
            <option key={i} value={lvl ?? ''}>
              {lvl ?? '(null)'}
            </option>
          ))}
        </select>

        <select
          value={queryParams.serviceName || ''}
          onChange={e =>
            setQueryParams(p => ({ ...p, serviceName: e.target.value }))
          }
        >
          <option value="">All Services</option>
          {metadata?.services?.map((s, i) => (
            <option key={i} value={s ?? ''}>
              {s ?? '(null)'}
            </option>
          ))}
        </select>

        <select
          value={queryParams.hostname || ''}
          onChange={e =>
            setQueryParams(p => ({ ...p, hostname: e.target.value }))
          }
        >
          <option value="">All Hosts</option>
          {metadata?.hostnames?.map((h, i) => (
            <option key={i} value={h ?? ''}>
              {h ?? '(null)'}
            </option>
          ))}
        </select>

        <select
          value={queryParams.environment || ''}
          onChange={e =>
            setQueryParams(p => ({ ...p, environment: e.target.value }))
          }
        >
          <option value="">All Environments</option>
          {metadata?.environments?.map((env, i) => (
            <option key={i} value={env ?? ''}>
              {env ?? '(null)'}
            </option>
          ))}
        </select>

        <button onClick={applyFilters} disabled={loading}>
          Apply Filters
        </button>
      </div>

      <div className="controls-bar">
        <button
          className="btn btn-primary"
          onClick={fetchLatestLogs}
          disabled={loading}
        >
          {loading ? 'Refreshing...' : '↻ Fetch Latest Logs'}
        </button>
      </div>

      <div className="log-container">
        <HistogramChart data={histogram} />
        {lastUpdatedRef.current && (
          <span className="last-updated">
            Last updated: {lastUpdatedRef.current.toLocaleString()}
          </span>
        )}
        <LogViewer logs={logs} />
        {loading && <div className="loading-text">Loading more logs...</div>}
      </div>
    </div>
  );
}

export default LogExplorer;
