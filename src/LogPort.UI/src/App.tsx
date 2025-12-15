import { Routes, Route } from 'react-router-dom'
import LogExplorer from './lib/pages/LogExplorer'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<LogExplorer />} />
      <Route path="/logs" element={<LogExplorer />} />
    </Routes>
  )
}
