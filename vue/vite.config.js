import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server:{
    hmr:true,
    port:8081,
    proxy:{
      '/api':{
        target:"http://localhost:5008",
        secure:false,
        changeOrigin:true,
        PathRewrite:{
          '^/api':'/api',
        },
      },
    },
  },
})
