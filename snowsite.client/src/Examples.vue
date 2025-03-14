<template>
  <div>
    <h1>Градиент</h1>
    <button @click="redirectToPixelFade">Перейти на HTML страницу</button>
    <h1>Пузырьки</h1>
    <button @click="redirectToBubbles">Перейти на HTML страницу</button>
    <h1>Карточки</h1>
    <button @click="redirectToCard">Перейти на HTML страницу</button>
    <h1>Космос</h1>
    <button @click="redirectToSpace">Перейти на HTML страницу</button>
  </div>
</template>
  
  <script scoped>
export default {
  methods: {
    redirectToPixelFade() {
      window.location.href = './src/html/PixelFade.html'; // Замените на ваш путь
    },
    redirectToBubbles() {
      window.location.href = './src/html/BackgroundBubbles.html'; // Замените на ваш путь
    },
    redirectToCard() {
      window.location.href = './src/html/Card.html'; // Замените на ваш путь
    },
    redirectToSpace() {
  window.location.href = './src/html/Space.html'; // Замените на ваш путь
}
  }
}
</script>
