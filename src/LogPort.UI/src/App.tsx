import { useState, useEffect, useRef } from 'react'
import './App.css'
import { LogViewer } from './components/logViewer'
import type { LogEntry, LogQueryParameters } from './lib/types/log'
import { getLogs } from './lib/services/logs.service'

function App() {
  const [logs, setLogs] = useState<LogEntry[]>([])
  const lastUpdatedRef = useRef<Date | null>(null)
  const [loading, setLoading] = useState(false)

  // Fetch initial logs on mount
  useEffect(() => {
    fetchLogs()
  }, [])

  const fetchLogs = async () => {
    setLoading(true)
    try {
      const params: LogQueryParameters = {
        from: lastUpdatedRef.current ? new Date(lastUpdatedRef.current.getTime() + 1) : undefined, // start just after last updated
        to: new Date(),
        pageSize: logs.length > 0 ? 99999 : 100,
      }

      const newLogs = await getLogs(params)
      if (newLogs && newLogs.length > 0) {
        console.log(`Fetched ${newLogs.length} new logs`)
        console.log(newLogs)
        setLogs(prev => [...prev, ...newLogs])
        // update lastUpdatedRef to newest log timestamp
        const latest = newLogs.reduce((max, log) => {
          const logDate = new Date(log.timestamp)
          return logDate > max ? logDate : max
        }, lastUpdatedRef.current ?? new Date(0))
        lastUpdatedRef.current = latest
      }
    } catch (err) {
      console.error('Failed to fetch logs', err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="fullscreen">
      <div style={{ display: 'flex', alignItems: 'center', marginBottom: '8px' }}>
        <button onClick={fetchLogs} disabled={loading} style={{ marginRight: '12px' }}>
          {loading ? 'Loading...' : 'Fetch New Logs'}
        </button>
        {lastUpdatedRef.current && <span>Last updated: {lastUpdatedRef.current.toLocaleString()}</span>}
      </div>
      <LogViewer logs={logs} />
    </div>
  )
}

export default App
