import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  return {
    plugins: [react()],
    server: {
      host: '127.0.0.1', port: 5173,
      proxy: { '/api': { target: env.API_PROXY_TARGET || 'https://localhost:7198', changeOrigin: true, secure: false } },
    },
    build: { outDir: '../wwwroot', emptyOutDir: true },
    test: { environment: 'jsdom', setupFiles: './src/test/setup.js', clearMocks: true },
  };
});
