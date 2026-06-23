import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// ASSUMPTION: Backend runs on https://localhost:7000 (ASP.NET Core default HTTPS port for .NET 10)
// If port differs, update the target below or read from .env
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5100',
        changeOrigin: true,
        secure: false,
        // TODO: Set to true and configure cert when using trusted dev cert
      },
    },
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
  },
})
