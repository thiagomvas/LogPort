import { useState, useEffect, useRef } from 'react'
import { LogViewer } from './components/logViewer'
import { placeholderLogs, type LogEntry, type LogQueryParameters } from './lib/types/log'
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
  const lastUpdatedRef = useRef<Date | null>(null)
  const wsRef = useRef<WebSocket | null>(null)

  // Initial fetch
  useEffect(() => {
    fetchLogs()
    return () => {
      wsRef.current?.close()
    }
  }, [])

  // Fetch logs from backend
  const fetchLogs = async () => {
    setLoading(true)
    try {
      const params: LogQueryParameters = {
        from: lastUpdatedRef.current ?? undefined,
        to: new Date(),
        pageSize: logs.length > 0 ? 99999 : 100,
      }

      const newLogs = await getLogs(params)
      if (newLogs && newLogs.length > 0) {
        console.log(`Fetched ${newLogs.length} new logs`)

        setLogs(prevLogs => [...newLogs, ...prevLogs])


        // Update lastUpdatedRef using the latest timestamp
        const latest = newLogs.reduce((max, log) => {
          const ts = log.timestamp ? new Date(log.timestamp) : new Date(0)
          return ts > max ? ts : max
        }, lastUpdatedRef.current ?? new Date(0))
        lastUpdatedRef.current = latest
      }

      const histogramParams: LogQueryParameters = {
        from: new Date(Date.now() - 1000 * 60 * 60 * 2), // last 1 hour
        to: new Date(),
        interval: '00:05:00',
      }
      const histogramData = await getHistogramData({})
      console.log(histogramData)
      setHistogram(histogramData)

    } catch (err) {
      console.error('Failed to fetch logs', err)
    } finally {
      setLoading(false)
    }
  }

  // Enable WebSocket tailing
  const enableTailing = () => {
    if (tailing) return // already enabled

    // Build WebSocket URL from env
    const wsProtocol = import.meta.env.USE_SSL === 'true' ? 'wss' : 'ws'
    const wsHost = import.meta.env.LOGPORT_AGENT_URL // host:port
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
        setLogs(prevLogs => [...newLogs, ...prevLogs])

      } catch (err) {
        console.error('Failed to parse log', err)
      }
    }

    setTailing(true)
  }

  return (
    <>
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


        <div className='log-container'>
          <HistogramChart data={histogram} />
          <LogViewer logs={logs} />
        </div>
      </div>
    </>

  )
}

export default App
