import { Routes, Route } from 'react-router-dom';
import LogExplorer from './pages/LogExplorer';
import LogTailPage from './pages/LogTail';
import Layout from './components/layout';
import DashboardPage from './pages/DashboardPage';

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/logs" element={<LogExplorer />} />
        <Route path="/logs/tail" element={<LogTailPage />} />
      </Routes>
    </Layout>
  );
}
