<!-- CircleComponent.vue -->
<template>
  <div class="circle-container">
    <!-- Кружки (планеты) -->
    <div v-for="(circle, index) in circles"
         :key="index"
         class="circle"
         :class="{ 'planet': true, 'sun': !circle.isOrbiting }"
         :style="{
        width: circle.size + 'px',
        height: circle.size + 'px',
        background: circle.background,
        left: circle.position.x + 'px',
        top: circle.position.y + 'px',
        transform: `translate(-50%, -50%) rotate(${circle.rotation}deg)`
      }">
      <div class="planet-surface"></div>
      <span class="planet-text">{{ circle.text }}</span>
    </div>

    <!-- Орбиты и соединения -->
    <svg class="orbits" width="100%" height="100%">
      <circle v-for="(circle, index) in circles.filter(c => c.isOrbiting)"
              :key="'orbit-' + index"
              :cx="circles[circle.parentIndex].position.x"
              :cy="circles[circle.parentIndex].position.y"
              :r="circle.orbitRadius"
              fill="none"
              stroke="rgba(255, 255, 255, 0.05)"
              stroke-width="1"
              stroke-dasharray="5 5" />
      <line v-for="(connection, index) in getConnections"
            :key="'line-' + index"
            :x1="connection.from.x"
            :y1="connection.from.y"
            :x2="connection.to.x"
            :y2="connection.to.y"
            stroke="rgba(255, 255, 255, 0.2)"
            stroke-width="1" />
    </svg>

    <!-- Звезды -->
    <div class="stars-layer"></div>
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
            background: 'radial-gradient(circle at 30% 30%, #ff8c00 0%, #ff4500 70%, #8b0000 100%)',
            size: 200,
            position: { x: 600, y: 400 },
            connectedTo: [1],
            isOrbiting: false,
            rotation: 0
          },
          {
            text: 'Средний',
            background: 'radial-gradient(circle at 40% 40%, #00ced1 0%, #20b2aa 60%, #006400 100%)',
            size: 150,
            position: { x: 0, y: 0 },
            connectedTo: [0, 2],
            isOrbiting: true,
            orbitRadius: 300,
            orbitSpeed: 8,
            parentIndex: 0,
            rotation: 0
          },
          {
            text: 'Маленький',
            background: 'radial-gradient(circle at 50% 50%, #87ceeb 0%, #4682b4 60%, #191970 100%)',
            size: 100,
            position: { x: 0, y: 0 },
            connectedTo: [1],
            isOrbiting: true,
            orbitRadius: 200,
            orbitSpeed: 5,
            parentIndex: 1,
            rotation: 0
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
                x: circle.position.x,
                y: circle.position.y
              };
              const toCenter = {
                x: this.circles[toIndex].position.x,
                y: this.circles[toIndex].position.y
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
      updatePositions() {
        this.circles.forEach((circle, index) => {
          if (circle.isOrbiting && circle.parentIndex !== undefined) {
            const parent = this.circles[circle.parentIndex];
            const angle = (Date.now() / 1000) * (2 * Math.PI / circle.orbitSpeed);

            circle.position.x = parent.position.x + circle.orbitRadius * Math.cos(angle);
            circle.position.y = parent.position.y + circle.orbitRadius * Math.sin(angle);

            circle.rotation = (Date.now() / 50) % 360;
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
    background: radial-gradient(circle at center, rgba(10, 20, 60, 0.8) 0%, rgba(5, 10, 30, 0.95) 50%, #000011 100%);
  }

  .planet {
    position: absolute;
    border-radius: 50%;
    overflow: hidden;
    display: flex;
    justify-content: center;
    align-items: center;
  }

  .sun {
    box-shadow: 0 0 40px rgba(255, 140, 0, 0.8), 0 0 80px rgba(255, 69, 0, 0.4), 0 0 120px rgba(255, 69, 0, 0.2);
  }

  .planet:not(.sun) {
    box-shadow: 0 0 15px rgba(255, 255, 255, 0.1), inset 0 0 20px rgba(0, 0, 0, 0.3);
  }

  .planet-surface {
    width: 100%;
    height: 100%;
    background: radial-gradient( circle at 70% 30%, rgba(255, 255, 255, 0.1) 0%, transparent 20% );
    position: absolute;
  }

  .planet-text {
    position: relative;
    color: white;
    font-family: Arial, sans-serif;
    font-size: 20px;
    text-align: center;
    z-index: 1;
    text-shadow: 0 0 5px rgba(0, 0, 0, 0.5);
  }

  /* Звезды с рандомным расположением и сиянием */
  .stars-layer {
    position: absolute;
    width: 100%;
    height: 100%;
    z-index: 0;
    background: transparent;
  }

    .stars-layer::before,
    .stars-layer::after {
      content: '';
      position: absolute;
      width: 100%;
      height: 100%;
    }

    .stars-layer::before {
      background: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100"><circle cx="50" cy="50" r="1" fill="white"/></svg>') repeat;
      background-size: 100px 100px;
      opacity: 0.7;
      animation: twinkle 3s infinite;
      filter: blur(0.5px);
    }

    .stars-layer::after {
      background: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="150" height="150"><circle cx="75" cy="75" r="0.5" fill="white"/></svg>') repeat;
      background-size: 150px 150px;
      background-position: 25px 25px;
      opacity: 0.5;
      animation: twinkle 4s infinite alternate;
      filter: blur(0.3px);
    }

  @keyframes twinkle {
    0% {
      opacity: 0.5;
    }

    50% {
      opacity: 1;
    }

    100% {
      opacity: 0.5;
    }
  }

  .orbits {
    position: absolute;
    top: 0;
    left: 0;
    pointer-events: none;
    z-index: 1;
  }

  .planet, .sun {
    z-index: 2;
  }
</style>
