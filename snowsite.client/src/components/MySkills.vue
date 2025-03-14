<!-- CircleComponent.vue -->
<template>
  <div class="circle-container">
    <!-- Кружки -->
    <div v-for="(circle, index) in circles"
         :key="index"
         class="circle"
         :style="{
        width: circle.size + 'px',
        height: circle.size + 'px',
        backgroundColor: circle.color
      }"
         ref="circleElements">
      {{ circle.text }}
    </div>

    <!-- SVG для линий -->
    <svg class="connections">
      <line v-for="(connection, index) in getConnections"
            :key="'line' + index"
            :x1="connection.x1"
            :y1="connection.y1"
            :x2="connection.x2"
            :y2="connection.y2"
            stroke="black"
            stroke-width="2" />
    </svg>
  </div>
</template>

<script>
  export default {
    name: 'CircleComponent',
    data() {
      return {
        circles: [
          {
            text: 'Большой',
            color: '#ff6b6b',
            size: 200,
            connections: [1] // Связь со средним кружком (индекс 1)
          },
          {
            text: 'Средний',
            color: '#4ecdc4',
            size: 150,
            connections: [0, 2] // Связь с большим (0) и маленьким (2)
          },
          {
            text: 'Маленький',
            color: '#45b7d1',
            size: 100,
            connections: [1] // Связь со средним (1)
          }
        ]
      }
    },
    computed: {
      getConnections() {
        const connections = [];
        const circleElements = this.$refs.circleElements;

        if (!circleElements) return connections;

        this.circles.forEach((circle, index) => {
          if (circle.connections) {
            const circleRect = circleElements[index].getBoundingClientRect();
            const circleCenterX = circleRect.left + circleRect.width / 2;
            const circleCenterY = circleRect.top + circleRect.height / 2;

            circle.connections.forEach(targetIndex => {
              const targetRect = circleElements[targetIndex].getBoundingClientRect();
              const targetCenterX = targetRect.left + targetRect.width / 2;
              const targetCenterY = targetRect.top + targetRect.height / 2;

              connections.push({
                x1: circleCenterX - circleElements[0].parentElement.getBoundingClientRect().left,
                y1: circleCenterY - circleElements[0].parentElement.getBoundingClientRect().top,
                x2: targetCenterX - circleElements[0].parentElement.getBoundingClientRect().left,
                y2: targetCenterY - circleElements[0].parentElement.getBoundingClientRect().top
              });
            });
          }
        });

        return connections;
      }
    },
    mounted() {
      // Обновление позиций после полной загрузки
      this.$forceUpdate();
    }
  }
</script>

<style scoped>
  .circle-container {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    gap: 20px;
    position: relative;
  }

  .circle {
    border-radius: 50%;
    display: flex;
    justify-content: center;
    align-items: center;
    color: white;
    font-family: Arial, sans-serif;
    font-size: 20px;
    text-align: center;
    position: relative;
  }

  .connections {
    position: absolute;
    color: black;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    z-index: -1;
  }
</style>
