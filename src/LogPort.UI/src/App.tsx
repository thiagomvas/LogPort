import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import { LogViewer } from './components/logViewer'
import type { LogEntry } from './lib/types/logEntry'

function App() {

  const logs: LogEntry[] = [
    {
      message: "User login successful",
      level: "info",
      timestamp: new Date(),
      serviceName: "AuthService",
    },
    {
      message: "Database connection error",
      level: "error",
      timestamp: new Date(),
      serviceName: "DatabaseService",
    },
    {
      message: "Payment processed",
      level: "info",
      timestamp: new Date(),
      serviceName: "PaymentService",
    },
    {
      message: "Cache miss for key user_123",
      level: "debug",
      timestamp: new Date(),
      serviceName: "CacheService",
    },
  ]

  return (
    <>
      <div className='fullscreen'>
        <LogViewer logs={logs} />
      </div>
    </>
  )
}

export default App
