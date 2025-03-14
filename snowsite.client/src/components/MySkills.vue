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
        backgroundColor: circle.color,
        position: 'absolute',
        left: circle.position.x + 'px',
        top: circle.position.y + 'px',
        animation: getAnimationStyle(index)
      }">
      {{ circle.text }}
    </div>

    <!-- Линии между кружками -->
    <svg class="connections" width="100%" height="100%">
      <line v-for="(connection, index) in getConnections"
            :key="'line-' + index"
            :x1="connection.from.x"
            :y1="connection.from.y"
            :x2="connection.to.x"
            :y2="connection.to.y"
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
            position: { x: 500, y: 300 },
            connectedTo: [1],
            isOrbiting: false,
            orbitRadius: 0,
            orbitSpeed: 0
          },
          {
            text: 'Средний',
            color: '#4ecdc4',
            size: 150,
            position: { x: 0, y: 0 }, // Начальная позиция будет вычисляться
            connectedTo: [0, 2],
            isOrbiting: true,
            orbitRadius: 300,
            orbitSpeed: 8,
            parentIndex: 0
          },
          {
            text: 'Маленький',
            color: '#45b7d1',
            size: 100,
            position: { x: 0, y: 0 }, // Начальная позиция будет вычисляться
            connectedTo: [1],
            isOrbiting: true,
            orbitRadius: 200,
            orbitSpeed: 5,
            parentIndex: 1
          }
        ],
        animationFrame: 0
      }
    },
    computed: {
      getConnections() {
        const connections = [];
        this.circles.forEach((circle, fromIndex) => {
          if (circle.connectedTo) {
            circle.connectedTo.forEach(toIndex => {
              const fromCenter = {
                x: circle.position.x + circle.size / 2,
                y: circle.position.y + circle.size / 2
              };
              const toCenter = {
                x: this.circles[toIndex].position.x + this.circles[toIndex].size / 2,
                y: this.circles[toIndex].position.y + this.circles[toIndex].size / 2
              };
              connections.push({
                from: fromCenter,
                to: toCenter
              });
            });
          }
        });
        return connections;
      }
    },
    methods: {
      getAnimationStyle(index) {
        const circle = this.circles[index];
        if (!circle.isOrbiting) return '';
        return `orbit-${index} ${circle.orbitSpeed}s infinite linear`;
      },
      updatePositions() {
        this.circles.forEach((circle, index) => {
          if (circle.isOrbiting && circle.parentIndex !== undefined) {
            const parent = this.circles[circle.parentIndex];
            const angle = (Date.now() / 1000) * (2 * Math.PI / circle.orbitSpeed);
            circle.position.x = parent.position.x +
              circle.orbitRadius * Math.cos(angle) - circle.size / 2;
            circle.position.y = parent.position.y +
              circle.orbitRadius * Math.sin(angle) - circle.size / 2;
          }
        });
        this.animationFrame = requestAnimationFrame(this.updatePositions);
      }
    },
    mounted() {
      this.updatePositions();
    },
    beforeDestroy() {
      cancelAnimationFrame(this.animationFrame);
    }
  }
</script>

<style scoped>
  .circle-container {
    position: relative;
    height: 100vh;
    overflow: hidden;
    background: #f0f0f0;
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
  }

  .connections {
    position: absolute;
    top: 0;
    left: 0;
    pointer-events: none;
  }

  /* Добавляем keyframes для каждого орбитального кружка */
  @keyframes orbit-1 {
    from {
      transform: rotate(0deg) translateX(300px) rotate(0deg);
    }

    to {
      transform: rotate(360deg) translateX(300px) rotate(-360deg);
    }
  }

  @keyframes orbit-2 {
    from {
      transform: rotate(0deg) translateX(200px) rotate(0deg);
    }

    to {
      transform: rotate(360deg) translateX(200px) rotate(-360deg);
    }
  }
</style>
