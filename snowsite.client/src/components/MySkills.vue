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
      <img v-if="circle.image" :src="circle.image" class="planet-image" :alt="circle.text" />
      <span v-else class="planet-text">{{ circle.text }}</span>
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

    <!-- Слои звезд -->
    <div class="stars-layer distant-stars"></div>
    <div class="stars-layer mid-stars"></div>
    <div class="stars-layer near-stars"></div>


  </div>
</template>
          { name: "HTML", icon: "/src/assets/icons/html.png" },
          { name: "CSS", icon: "/src/assets/icons/css.png" },
          { name: "JavaScript", icon: "/src/assets/icons/js.png" },
          { name: ".NET", icon: "/src/assets/icons/dotnet.png" },
          { name: "Unity", icon: "/src/assets/icons/unity.png" },
          { name: "C#", icon: "/src/assets/icons/cSharp.png" },
          { name: "Visual Studio", icon: "/src/assets/icons/vs.png" },
          { name: "ASP.NET", icon: "/src/assets/icons/asp.png" },
<script>
  export default {
    name: 'CircleComponent',
    data() {
      return {
        circles: [
          {
            text: '',
            background: 'radial-gradient(circle at 30% 30%, #ff8c00 0%, #ff4500 70%, #8b0000 100%)',
            image: '/src/assets/icons/cSharp.png',
            size: 100,
            position: { x: 500, y: 300 },
            connectedTo: [1],
            isOrbiting: false,
            rotation: 0,
            velocity: { x: 0, y: 0 }
          },
          {
            text: '',
            background: '#5C2D91',
            image: '/src/assets/icons/dotnet.png',
            size: 75,
            position: { x: 0, y: 0 },
            connectedTo: [0, 2],
            isOrbiting: true,
            orbitRadius: 200,
            orbitSpeed: 40,
            orbitDirection: 1, // 1 = по часовой, -1 = против часовой
            parentIndex: 0,
            rotation: 0,
            velocity: { x: 0, y: 0 },
            angle: 0 // Текущий угол для равномерного распределения
          },
          {
            text: '',
            background: 'radial-gradient(circle at 50% 50%, #87ceeb 0%, #4682b4 60%, #191970 100%)',
            image: '/src/assets/icons/html.png',
            size: 50,
            position: { x: 0, y: 0 },
            connectedTo: [1],
            isOrbiting: true,
            orbitRadius: 100,
            orbitSpeed: 20,
            orbitDirection: 1,
            parentIndex: 1,
            rotation: 0,
            velocity: { x: 0, y: 0 },
            angle: 0
          },
          {
            text: '',
            background: 'radial-gradient(circle at 50% 50%, #87ceeb 0%, #4682b4 60%, #191970 100%)',
            image: '/src/assets/icons/css.png',
            size: 50,
            position: { x: 0, y: 0 },
            connectedTo: [1],
            isOrbiting: true,
            orbitRadius: 100,
            orbitSpeed: 20,
            orbitDirection: 1,
            parentIndex: 1,
            rotation: 0,
            velocity: { x: 0, y: 0 },
            angle: 0
          }, {
            text: '',
            background: 'radial-gradient(circle at 50% 50%, #87ceeb 0%, #4682b4 60%, #191970 100%)',
            image: '/src/assets/icons/js.png',
            size: 50,
            position: { x: 0, y: 0 },
            connectedTo: [1],
            isOrbiting: true,
            orbitRadius: 100,
            orbitSpeed: 20,
            orbitDirection: 1,
            parentIndex: 1,
            rotation: 0,
            velocity: { x: 0, y: 0 },
            angle: 0
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
              const fromCenter = { x: circle.position.x, y: circle.position.y };
              const toCenter = { x: this.circles[toIndex].position.x, y: this.circles[toIndex].position.y };
              connections.push({ from: fromCenter, to: toCenter });
            });
          }
        });
        return connections;
      }
    },
    methods: {
      updatePositions() {
        // Обновление орбитального движения и углов
        this.circles.forEach((circle, index) => {
          if (circle.isOrbiting && circle.parentIndex !== undefined) {
            const parent = this.circles[circle.parentIndex];

            // Обновляем угол с учетом направления
            circle.angle += (circle.orbitDirection * 2 * Math.PI) / (circle.orbitSpeed * 60); // 60 кадров в секунду

            // Идеальная позиция на орбите
            const idealX = parent.position.x + circle.orbitRadius * Math.cos(circle.angle);
            const idealY = parent.position.y + circle.orbitRadius * Math.sin(circle.angle);

            // Плавное стремление к идеальной позиции
            circle.position.x += (idealX - circle.position.x) * 0.1 + circle.velocity.x;
            circle.position.y += (idealY - circle.position.y) * 0.1 + circle.velocity.y;
            //circle.rotation = (Date.now() / 50) % 360;
          }
        });

        // Обработка столкновений и отталкивания
        for (let i = 0; i < this.circles.length; i++) {
          for (let j = i + 1; j < this.circles.length; j++) {
            const circle1 = this.circles[i];
            const circle2 = this.circles[j];

            const dx = circle2.position.x - circle1.position.x;
            const dy = circle2.position.y - circle1.position.y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            const minDistance = (circle1.size + circle2.size) / 2 + 20; // Добавляем небольшой отступ

            if (distance < minDistance) {
              const nx = dx / distance || 0; // Избегаем деления на 0
              const ny = dy / distance || 0;
              const overlap = minDistance - distance;
              const repulsionForce = overlap * 0.1;

              if (circle1.isOrbiting) {
                circle1.velocity.x -= nx * repulsionForce;
                circle1.velocity.y -= ny * repulsionForce;
              }
              if (circle2.isOrbiting) {
                circle2.velocity.x += nx * repulsionForce;
                circle2.velocity.y += ny * repulsionForce;
              }

            }
          }
        }

        // Затухание скорости
        this.circles.forEach(circle => {
          circle.velocity.x *= 0.9;
          circle.velocity.y *= 0.9;
        });

        this.animationFrame = requestAnimationFrame(this.updatePositions);
      },
      initializePositions() {
        // Устанавливаем начальные позиции без пересечений
        this.circles.forEach((circle, index) => {
          if (circle.isOrbiting && circle.parentIndex !== undefined) {
            const parent = this.circles[circle.parentIndex];
            circle.angle = (index - 1) * Math.PI; // Разница 180°
            circle.position.x = parent.position.x + circle.orbitRadius * Math.cos(circle.angle);
            circle.position.y = parent.position.y + circle.orbitRadius * Math.sin(circle.angle);

            // Проверка и коррекция пересечений
            for (let i = 0; i < index; i++) {
              const other = this.circles[i];
              const dx = circle.position.x - other.position.x;
              const dy = circle.position.y - other.position.y;
              const distance = Math.sqrt(dx * dx + dy * dy);
              const minDistance = (circle.size + other.size) / 2 + 20;

              if (distance < minDistance) {
                const nx = dx / distance || 0;
                const ny = dy / distance || 0;
                const correction = minDistance - distance;
                circle.position.x = other.position.x + nx * minDistance;
                circle.position.y = other.position.y + ny * minDistance;
              }
            }
          }
        });
      }
    },

    mounted() {
      this.initializePositions();
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
    height: 100%;
    overflow: hidden;
    background: linear-gradient(to bottom, #000011 0%, #000022 50%, #000033 100%);
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
    background: radial-gradient(circle at 70% 30%, rgba(255, 255, 255, 0.1) 0%, transparent 20%);
    position: absolute;
  }

  .planet-text {
    position: relative;
    color: white;
    font-family: Arial, sans-serif;
    font-size: 12px;
    text-align: center;
    z-index: 1;
    text-shadow: 0 0 5px rgba(0, 0, 0, 0.5);
  }

  .planet-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
    position: absolute;
    z-index: 1;
  }

  .stars-layer {
    position: absolute;
    width: 100%;
    height: 100%;
    z-index: 0;
  }

  .distant-stars {
    background: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="200" height="200"><circle cx="100" cy="100" r="0.3" fill="white"/></svg>') repeat;
    opacity: 0.6;
    filter: blur(0.5px);
    animation: twinkle 6s infinite;
  }

  .mid-stars {
    background: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="150" height="150"><circle cx="75" cy="75" r="0.7" fill="white"/></svg>') repeat;
    background-position: 50px 50px;
    opacity: 0.8;
    filter: blur(0.3px);
    animation: twinkle 4s infinite alternate;
  }

  .near-stars {
    background: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="300" height="300"><circle cx="150" cy="150" r="1" fill="white"/></svg>') repeat;
    background-position: 100px 100px;
    opacity: 1;
    filter: blur(0.2px);
    animation: twinkle 3s infinite;
  }

    .near-stars::after {
      content: '';
      position: absolute;
      width: 100%;
      height: 100%;
      background: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="500" height="500"><circle cx="250" cy="250" r="1.5" fill="white"/></svg>') repeat;
      background-position: 75px 75px;
      opacity: 0.9;
      filter: blur(0.1px);
      animation: twinkle-bright 5s infinite;
    }

  .direction-toggle {
    position: absolute;
    top: 10px;
    right: 10px;
    padding: 10px 20px;
    background-color: rgba(255, 255, 255, 0.2);
    border: none;
    border-radius: 5px;
    color: white;
    cursor: pointer;
    z-index: 3;
    transition: background-color 0.3s;
  }

    .direction-toggle:hover {
      background-color: rgba(255, 255, 255, 0.4);
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

  @keyframes twinkle-bright {
    0% {
      opacity: 0.7;
    }

    50% {
      opacity: 1;
    }

    100% {
      opacity: 0.7;
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
