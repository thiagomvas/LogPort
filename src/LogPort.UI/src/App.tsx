import { Routes, Route } from 'react-router-dom'
import LogExplorer from './lib/pages/LogExplorer'
import LogTailPage from './lib/pages/LogTail'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<LogExplorer />} />
      <Route path="/logs" element={<LogExplorer />} />
      <Route path="/logs/tail" element={<LogTailPage />} />

    </Routes>
  )
}
