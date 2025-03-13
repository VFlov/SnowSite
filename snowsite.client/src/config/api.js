const ENV = process.env.NODE_ENV || 'development';

const config = {
  development: {
    apiBaseUrl: 'http://vflov.ru:5020', // Для dev используем порт 5020
    frontendPort: 65311
  },
  production: {
    apiBaseUrl: '', // Оставляем пустым для относительного пути через Nginx
    frontendPort: 443
  }
};

export const API_CONFIG = config[ENV];

export const getApiUrl = (path) => {
  if (path.startsWith('http')) return path;
  return `${API_CONFIG.apiBaseUrl}${path.startsWith('/') ? '' : '/'}${path}`;
};
