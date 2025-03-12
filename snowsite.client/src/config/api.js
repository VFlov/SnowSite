
const ENV = process.env.NODE_ENV || 'development';

const config = {
  development: {
    apiBaseUrl: 'https://45.130.214.139:5020',
    frontendPort: 65311
  },
  production: {
    apiBaseUrl: 'https://45.130.214.139:5000',
    frontendPort: 80
  }
};

export const API_CONFIG = config[ENV];

// Функция для получения относительного пути API
export const getApiUrl = (path) => {
  // Если путь уже полный URL, возвращаем его как есть
  if (path.startsWith('http')) return path;
  // Иначе строим относительный путь от базового URL
  return `${API_CONFIG.apiBaseUrl}${path.startsWith('/') ? '' : '/'}${path}`;
};
