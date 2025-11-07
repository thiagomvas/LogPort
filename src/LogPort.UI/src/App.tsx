import { useState, useEffect, useRef } from 'react'
import { LogViewer } from './components/logViewer'
import { type LogEntry, type LogQueryParameters } from './lib/types/log'
import { getLogs, normalizeLog } from './lib/services/logs.service'
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

  const lastUpdatedRef = useRef<Date | null>(null)
  const wsRef = useRef<WebSocket | null>(null)
  const loadingRef = useRef(false)        // prevent concurrent fetches
  const pageRef = useRef(1)               // ✅ track current page synchronously

  // keep pageRef in sync with state
  useEffect(() => {
    pageRef.current = page
  }, [page])

  // Initial fetch
  useEffect(() => {
    fetchLogs()
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
      const currentPage = pageRef.current
      const params: LogQueryParameters = {
        page: currentPage,
        pageSize: 100,
      }

      const newLogs = await getLogs(params)
      if (!newLogs || newLogs.length === 0) {
        console.log('No more logs available')
        setHasMore(false)
        return
      }

      console.log(`Fetched ${newLogs.length} logs (page ${currentPage})`)
      setLogs(prev => [...prev, ...newLogs])
      setPage(prev => prev + 1) // will update pageRef via useEffect

      // Track latest timestamp
      const latest = newLogs.reduce((max, log) => {
        const ts = log.timestamp ? new Date(log.timestamp) : new Date(0)
        return ts > max ? ts : max
      }, lastUpdatedRef.current ?? new Date(0))
      lastUpdatedRef.current = latest

      // Refresh histogram occasionally
      if (currentPage === 1 || currentPage % 3 === 0) {
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

      <div className="log-container">
        <HistogramChart data={histogram} />
        <LogViewer logs={logs} />
        {loading && <div style={{ textAlign: 'center', margin: '10px' }}>Loading more logs...</div>}
      </div>
    </div>
  )
}

export default App
