import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import vuePlugin from '@vitejs/plugin-vue';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const isProduction = process.env.NODE_ENV === 'production';

// Настройки портов для разных окружений
const ports = {
  development: {
    frontend: 65311,
    api: 5020
  },
  production: {
    frontend: 80,
    api: 5000
  }
};

const currentPorts = isProduction ? ports.production : ports.development;

// Настройки сертификатов (только для разработки)
const baseFolder = env.APPDATA ? `${env.APPDATA}/ASP.NET/https` : `${env.HOME}/.aspnet/https`;
const certificateName = "snowsite.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!isProduction) {
  if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true });
  }

  if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
      'dev-certs',
      'https',
      '--export-path',
      certFilePath,
      '--format',
      'Pem',
      '--no-password',
    ], { stdio: 'inherit' }).status) {
      throw new Error("Could not create certificate.");
    }
  }
}

export default defineConfig({
  plugins: [vuePlugin()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    proxy: {
      '/api': {
        target: `http://localhost:${currentPorts.api}`,
        changeOrigin: true,
        secure: false,
        // Убираем префикс /api при проксировании если нужно
        rewrite: (path) => path.replace(/^\/api/, '')
      }
    },
    host: '0.0.0.0',
    port: currentPorts.frontend,
    // Используем HTTPS только в разработке
    ...(isProduction ? {} : {
      https: {
        key: fs.readFileSync(keyFilePath),
        cert: fs.readFileSync(certFilePath),
      }
    })
  },
  build: {
    // Указываем порт для production сборки
    outDir: 'dist',
    assetsDir: 'assets',
  }
});
