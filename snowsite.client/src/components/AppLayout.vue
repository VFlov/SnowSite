<!-- src/layouts/AppLayout.vue -->
<template>
  <div class="app-layout">
    <!-- Шапка -->
    <header class="header">
      <nav class="navbar">
        <div class="logo">
          <router-link to="/">Мое приложение</router-link>
        </div>
        <ul class="nav-links">
          <li><router-link to="/">Главная</router-link></li>
          <li><router-link to="/map">Карта</router-link></li>
          <!-- Добавьте другие ссылки по необходимости -->
        </ul>
      </nav>
    </header>

    <!-- Основной контент -->
    <main class="main-content">
      <slot></slot> <!-- Здесь будет рендериться содержимое дочерних компонентов -->
    </main>

    <!-- Футер -->
    <footer class="footer">
      <p>&copy; 2025 Мое приложение. Все права защищены.</p>
    </footer>
  </div>
</template>

<script>
  export default {
    name: 'AppLayout'
  };
</script>

<style scoped>
  .app-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }

  .header {
    background-color: #2c3e50;
    padding: 1rem;
    color: white;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }

  .navbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    max-width: 1200px;
    margin: 0 auto;
  }

  .logo a {
    color: white;
    font-size: 1.5rem;
    text-decoration: none;
    font-weight: bold;
  }

  .nav-links {
    list-style: none;
    display: flex;
    gap: 1.5rem;
    margin: 0;
    padding: 0;
  }

    .nav-links li a {
      color: white;
      text-decoration: none;
      font-size: 1rem;
    }

      .nav-links li a:hover {
        color: #42b983;
      }

  .main-content {
    flex: 1;
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
    width: 100%;
  }

  .footer {
    background-color: #f8f8f8;
    padding: 1rem;
    text-align: center;
    border-top: 1px solid #e7e7e7;
  }

  @media (max-width: 768px) {
    .navbar {
      flex-direction: column;
      text-align: center;
    }

    .nav-links {
      flex-direction: column;
      gap: 1rem;
      margin-top: 1rem;
    }
  }
</style>
