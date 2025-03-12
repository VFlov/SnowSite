const ENV = process.env.NODE_ENV || 'development';

const config = {
  development: {
    apiBaseUrl: 'http://vflov.ru:5020', // Для dev используем порт 65311
    frontendPort: 65311
  },
  production: {
    apiBaseUrl: 'https://vflov.ru:5000', // HTTPS для продакшн
    frontendPort: 443
  }
};

export const API_CONFIG = config[ENV];

export const getApiUrl = (path) => {
  if (path.startsWith('http')) return path;
  return `${API_CONFIG.apiBaseUrl}${path.startsWith('/') ? '' : '/'}${path}`;
};
