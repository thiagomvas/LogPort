import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  define: {
    'import.meta.env.LOGPORT_AGENT_URL': JSON.stringify(process.env.LOGPORT_AGENT_URL),
    'import.meta.env.LOGPORT_USER': JSON.stringify(process.env.LOGPORT_USER),
    'import.meta.env.LOGPORT_PASS': JSON.stringify(process.env.LOGPORT_PASS),
  },
});
