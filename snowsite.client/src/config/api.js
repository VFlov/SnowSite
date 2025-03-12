const ENV = process.env.NODE_ENV || 'development';

const config = {
  development: {
    apiBaseUrl: 'http://vflov.ru:65311', // Для dev используем порт 65311
    frontendPort: 65311
  },
  production: {
    apiBaseUrl: 'https://vflov.ru', // HTTPS для продакшн
    frontendPort: 443
  }
};

export const API_CONFIG = config[ENV];

export const getApiUrl = (path) => {
  if (path.startsWith('http')) return path;
  return `${API_CONFIG.apiBaseUrl}${path.startsWith('/') ? '' : '/'}${path}`;
};
