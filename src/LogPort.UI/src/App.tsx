import { useState, useEffect, useRef } from 'react'
import './App.css'
import { LogViewer } from './components/logViewer'
import type { LogEntry } from './lib/types/logEntry'

function App() {
  const [logs, setLogs] = useState<LogEntry[]>([])
  const wsRef = useRef<WebSocket | null>(null)

  useEffect(() => {
    // Open WebSocket connection
    wsRef.current = new WebSocket('ws://localhost:8080/api/live-logs')

    wsRef.current.onopen = () => {
      console.log('WebSocket connected')
    }

    wsRef.current.onmessage = (event) => {
      try {
        const newLogs: LogEntry[] = JSON.parse(event.data)

        setLogs((prevLogs) => [...newLogs, ...prevLogs]) // prepend to current state safely
      } catch (err) {
        console.error('Failed to parse log', err)
      }
    }

    wsRef.current.onerror = (err) => {
      console.error('WebSocket error', err)
    }

    wsRef.current.onclose = () => {
      console.log('WebSocket disconnected')
    }

    return () => {
      wsRef.current?.close()
    }
  }, [])

  return (
    <div className="fullscreen">
      <LogViewer logs={logs} />
    </div>
  )
}

export default App
