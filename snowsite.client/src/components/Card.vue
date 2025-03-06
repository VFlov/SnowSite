<template>
  <div>
    <div class="container">
      <div class="box" :style="gradientStyle">
        <b class="name">Комната<br><h2>{{ item.Name }}</h2></b>
        <p class="count">{{ item.ParticipantCount }} Участников</p>
        <button @click="handleClick">Присоединиться</button>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    item: {
      type: Object, // Изменяем тип на Object
      required: true,
      default: () => ({ Name: '', ParticipantCount: 0 })
    }
  },
  data() {
    return {
      gradientStyle: {}
    };
  },
  mounted() {
    this.generateRandomGradient();
  },
  methods: {
    getRandomColor() {
      const letters = '0123456789ABCDEF';
      let color = '#';
      for (let i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
      }
      return color;
    },
    generateRandomGradient() {
      const color1 = this.getRandomColor();
      const color2 = this.getRandomColor();
      const angle = Math.floor(Math.random() * 360);
      this.gradientStyle = {
        background: `linear-gradient(${angle}deg, ${color1}, ${color2})`
      };
    },
    handleClick() {
      this.$emit('my-event', this.item.Name); // Передаём только имя комнаты
    }
  }
};
</script>

<style scoped>
  * {
    margin: 0px;
    padding: 0px;
    box-sizing: border-box;
    font-family: "Poppins", serif;
  }

  body {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    background: #1d031f;
  }

  .container {
    position: relative;
    display: flex;
    justify-content: center;
    align-items: center;
    flex-wrap: wrap;
    padding: 50px;
    gap: 50px;
  }

  .box {
    position: relative;
    height: 400px;
    width: 280px;
    background: #fff;
    border-radius: 20px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
  }

    .box::before {
      content: "";
      position: absolute;
      inset: 0;
      border-radius: 20px;
      background: v-bind(gradientStyle);
    }

    .box::after {
      content: "";
      position: absolute;
      inset: 0;
      border-radius: 20px;
      background: v-bind(gradientStyle);
      filter: blur(16px);
    }

    .box::before,
    .box::after {
      z-index: 1;
    }

    .box b {
      padding: 30px;
      position: absolute;
      display: block;
      inset: 4px;
      border-radius: 16px;
      background: rgba(0, 0, 0, 0.75);
      z-index: 2;
    }

      .box b p {
        font-weight: 200;
        text-shadow: 0 0 15px #fff;
        z-index: 2;
      }

    .box button {
      padding: 10px 20px;
      background-color: #70abaf;
      color: #e5dcdc;
      border: none;
      border-radius: 5px;
      cursor: pointer;
      z-index: 2;
    }

  .name {
    padding: 30px;
    position: relative;
    z-index: 2;
    border-radius: 16px;
    background: rgba(0, 0, 0, 0.75);
    color: #fff;
    font-weight: bold;
    display: block;
    text-shadow: 0 0 5px #fff;
  }

  .count {
    position: relative;
    z-index: 2;
    color: #ddd;
    font-weight: 200;
    text-shadow: 0 0 5px #ddd;
  }
</style>
