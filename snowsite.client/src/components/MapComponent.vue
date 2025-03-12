<template>
  <div id="map" style="width: 100%; height: 90vh;"></div>
</template>

<script>
  import { getApiUrl } from '@/config/api';
export default {
  name: 'MapComponent',
  data() {
    return {
      houses: [],
      icons: {
        AAA: '/src/assets/icons/AAA.png',
        AA: '/src/assets/icons/AA.png',
        А: '/src/assets/icons/A.png',
        В: '/src/assets/icons/B.png',
        С: '/src/assets/icons/C.png',
        D: '/src/assets/icons/D.png',
        Е: '/src/assets/icons/E.png',
        E: '/src/assets/icons/E.png',
        F: '/src/assets/icons/F.png',
        G: '/src/assets/icons/G.png'
      }
    };
  },
  async mounted() {
    await this.fetchHouses();
    this.initMap();
  },
  methods: {
    async fetchHouses() {
      try {
        const response = await fetch(getApiUrl('/api/Map/gethouses'));
        const data = await response.json();
        this.houses = data;
        this.$emit('houses-loaded', this.houses.length);
      } catch (error) {
        console.error('Ошибка при загрузке данных:', error);
      }
    },
    initMap() {
      ymaps.ready(() => {
        const map = new ymaps.Map('map', {
          center: [55.76, 37.64],
          zoom: 10
        });

        // Создаем макет для иконки
        const iconLayout = ymaps.templateLayoutFactory.createClass(
          `<div style="position: relative; width: 30px; height: 44px;">
             <img src="{{ properties.icon }}"
                  style="position: absolute; top: 0; left: 0; width: 30px; height: 44px;">
           </div>`
        );

        // Создаем кластеризатор
        const clusterer = new ymaps.Clusterer({
          preset: 'islands#blueClusterIcons', // Встроенный стиль кластера
          clusterDisableClickZoom: false, // Разрешить зум при клике на кластер
          clusterHideIconOnBalloonOpen: false,
          groupByCoordinates: false,
          clusterOpenBalloonOnClick: true,
          // Настройка расстояния, при котором иконки объединяются в кластер (в пикселях)
          gridSize: 64, // Увеличьте или уменьшите для изменения чувствительности
          minClusterSize: 2 // Минимальное количество меток для создания кластера
        });

        // Создаем метки для домов
        const placemarks = this.houses.map(house => {
          const coordinates = [house.hplGeoLatitude, house.hplGeoLongitude];
          const hint = house.ihsEeClass;
          const balloonContent = `Класс: ${house.ihsEeClass}`;
          const icon = this.icons[house.ihsEeClass[0]] || 'path/to/default-icon.png';

          return new ymaps.Placemark(
            coordinates,
            {
              hintContent: hint,
              balloonContent: balloonContent,
              icon: icon
            },
            {
              iconLayout: iconLayout,
              iconShape: {
                type: 'Circle',
                coordinates: [0, 0],
                radius: 16
              }
            }
          );
        });

        // Добавляем метки в кластеризатор
        clusterer.add(placemarks);
        map.geoObjects.add(clusterer);

        // Опционально: настройка поведения при изменении масштаба
        let timeout;
        map.events.add('boundschange', () => {
          clearTimeout(timeout);
          timeout = setTimeout(() => {
            clusterer.options.set('gridSize', map.getZoom() < 12 ? 64 : 32);
          }, 100); // Задержка 100 мс
        });
      });
    }
  }
};
</script>

<style scoped>
  #map {
    width: 100%;
    height: 600px;
    margin-top: 20px;
    border-radius: 10px;
    box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
  }
</style>
