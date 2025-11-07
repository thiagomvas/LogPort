import { useState, useEffect, useRef } from 'react'
import { LogViewer } from './components/logViewer'
import { type LogEntry, type LogMetadata, type LogQueryParameters } from './lib/types/log'
import { getLogs, normalizeLog, getMetadata } from './lib/services/logs.service'
import type { LogBucket } from './lib/types/analytics'
import { getHistogramData } from './lib/services/analytics.service'
import { HistogramChart } from './components/histogram'
import './styles/logsPage.css'

function App() {
  const [logs, setLogs] = useState<LogEntry[]>([])
  const [histogram, setHistogram] = useState<LogBucket[]>([])
  const [loading, setLoading] = useState(false)
  const [tailing, setTailing] = useState(false)
  const [page, setPage] = useState(1)
  const [hasMore, setHasMore] = useState(true)
  const [metadata, setMetadata] = useState<LogMetadata | null>(null)

  const [queryParams, setQueryParams] = useState<LogQueryParameters>({
    page: 1,
    pageSize: 100,
    level: '',
    search: '',
    serviceName: '',
    hostname: '',
    environment: '',
  })

  const lastUpdatedRef = useRef<Date | null>(null)
  const wsRef = useRef<WebSocket | null>(null)
  const loadingRef = useRef(false)
  const pageRef = useRef(1)

  useEffect(() => {
    pageRef.current = page
  }, [page])

  // Initial fetch
  useEffect(() => {
    fetchLogs()
    getMetadata().then(setMetadata).catch(err => console.error('Failed to load metadata', err))
    return () => {
      wsRef.current?.close()
    }
  }, [])

  // Infinite scroll
  useEffect(() => {
    const handleScroll = () => {
      if (loadingRef.current || !hasMore) return

      const nearBottom =
        window.innerHeight + window.scrollY >= document.body.offsetHeight - 300

      if (nearBottom) {
        console.log('Reached bottom, fetching next page...')
        fetchLogs()
      }
    }

    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [hasMore])

  // Fetch logs
  const fetchLogs = async () => {
    if (loadingRef.current) return
    loadingRef.current = true
    setLoading(true)

    try {
      const params: LogQueryParameters = {
        ...queryParams,
        page: pageRef.current,
      }

      const newLogs = await getLogs(params)
      if (!newLogs || newLogs.length === 0) {
        console.log('No more logs available')
        setHasMore(false)
        return
      }

      console.log(`Fetched ${newLogs.length} logs (page ${pageRef.current})`)
      setLogs(prev => [...prev, ...newLogs])
      setPage(prev => prev + 1)

      // Track latest timestamp
      const latest = newLogs.reduce((max, log) => {
        const ts = log.timestamp ? new Date(log.timestamp) : new Date(0)
        return ts > max ? ts : max
      }, lastUpdatedRef.current ?? new Date(0))
      lastUpdatedRef.current = latest

      // Refresh histogram occasionally
      if (pageRef.current === 1 || pageRef.current % 3 === 0) {
        const histogramData = await getHistogramData({})
        setHistogram(histogramData)
      }
    } catch (err) {
      console.error('Failed to fetch logs', err)
    } finally {
      loadingRef.current = false
      setLoading(false)
    }
  }

  // Apply filters
  const applyFilters = () => {
    setLogs([])
    setHasMore(true)
    setPage(1)
    pageRef.current = 1
    fetchLogs()
  }

  // Enable WebSocket tailing
  const enableTailing = () => {
    if (tailing) return

    const wsProtocol = import.meta.env.USE_SSL === 'true' ? 'wss' : 'ws'
    const wsHost = import.meta.env.LOGPORT_AGENT_URL
    const wsUrl = `${wsProtocol}://${wsHost}/api/live-logs`

    wsRef.current = new WebSocket(wsUrl)

    wsRef.current.onopen = () => console.log('WebSocket tailing enabled')
    wsRef.current.onclose = () => console.log('WebSocket disconnected')
    wsRef.current.onerror = (err) => console.error('WebSocket error', err)
    wsRef.current.onmessage = (event) => {
      try {
        const rawLogs: any[] = JSON.parse(event.data)
        const newLogs: LogEntry[] = rawLogs.map(normalizeLog).reverse()

        console.log(`Received ${newLogs.length} live logs via WebSocket`)
        setLogs(prev => [...newLogs, ...prev])
      } catch (err) {
        console.error('Failed to parse log', err)
      }
    }

    setTailing(true)
  }

  return (
    <div>
      {/* --- Filter Controls --- */}
      <div
        className="filter-bar"
        style={{ display: 'flex', gap: '8px', marginBottom: '12px', flexWrap: 'wrap' }}
      >
        <input
          type="text"
          placeholder="Search logs..."
          value={queryParams.search || ''}
          onChange={(e) => setQueryParams(prev => ({ ...prev, search: e.target.value }))}
        />

        <select
          value={queryParams.level || ''}
          onChange={(e) => setQueryParams(prev => ({ ...prev, level: e.target.value }))}
        >
          <option value="">All Levels</option>
          {metadata?.logLevels?.map((lvl, i) => (
            <option key={i} value={lvl ?? ''}>{lvl ?? '(null)'}</option>
          ))}
        </select>

        <select
          value={queryParams.serviceName || ''}
          onChange={(e) => setQueryParams(prev => ({ ...prev, serviceName: e.target.value }))}
        >
          <option value="">All Services</option>
          {metadata?.services?.map((s, i) => (
            <option key={i} value={s ?? ''}>{s ?? '(null)'}</option>
          ))}
        </select>

        <select
          value={queryParams.hostname || ''}
          onChange={(e) => setQueryParams(prev => ({ ...prev, hostname: e.target.value }))}
        >
          <option value="">All Hosts</option>
          {metadata?.hostnames?.map((h, i) => (
            <option key={i} value={h ?? ''}>{h ?? '(null)'}</option>
          ))}
        </select>

        <select
          value={queryParams.environment || ''}
          onChange={(e) => setQueryParams(prev => ({ ...prev, environment: e.target.value }))}
        >
          <option value="">All Environments</option>
          {metadata?.environments?.map((env, i) => (
            <option key={i} value={env ?? ''}>{env ?? '(null)'}</option>
          ))}
        </select>

        <button onClick={applyFilters} disabled={loading}>
          Apply Filters
        </button>
      </div>

      {/* --- Control Buttons --- */}
      <div style={{ display: 'flex', alignItems: 'center', marginBottom: '8px' }}>
        <button onClick={fetchLogs} disabled={loading} style={{ marginRight: '12px' }}>
          {loading ? 'Loading...' : 'Fetch New Logs'}
        </button>
        <button onClick={enableTailing} disabled={tailing}>
          {tailing ? 'Tailing Enabled' : 'Enable Tailing'}
        </button>
        {lastUpdatedRef.current && (
          <span style={{ marginLeft: '12px' }}>
            Last updated: {lastUpdatedRef.current.toLocaleString()}
          </span>
        )}
      </div>

      {/* --- Main Log Section --- */}
      <div className="log-container">
        <HistogramChart data={histogram} />
        <LogViewer logs={logs} />
        {loading && <div style={{ textAlign: 'center', margin: '10px' }}>Loading more logs...</div>}
      </div>
    </div>
  )
}

export default App
