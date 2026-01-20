import { Routes, Route } from 'react-router-dom';
import LogExplorer from './pages/LogExplorer';
import LogTailPage from './pages/LogTail';
import Layout from './components/layout';
import DashboardPage from './pages/DashboardPage';
import AuthGuard from './components/authGuard';
import LoginPage from './pages/LoginPage';

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/" element={<AuthGuard><DashboardPage /></AuthGuard>} />
        <Route path="/logs" element={<AuthGuard><LogExplorer /></AuthGuard>} />
        <Route path="/logs/tail" element={<AuthGuard><LogTailPage /></AuthGuard>} />
      </Routes>
    </Layout>
  );
}
