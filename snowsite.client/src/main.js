
import { createApp } from 'vue'
import App from './App.vue'

import { createRouter, createWebHistory } from 'vue-router';
import Social from './SocialNetwork.vue';
import Main from './MainPage.vue';
import YandexMap from './YandexMap.vue';
import Examples from './Examples.vue';
import VideoCall from './VideoCall.vue';
import Valheim from './Valheim.vue';

//const Home = { template: Card }
//const SocialNetwork = { template: Social }

const routes = [
  { path: '/', name: 'card', component: Main },
  { path: '/home', redirect: '/' },
  { path: '/social', name: 'social', component: Social },
  { path: '/map', name : 'map', component: YandexMap },
  { path: '/examples', name : 'examples', component: Examples},
  { path: '/videocall', name: 'videocall', component: VideoCall },
  { path: '/valheim', name: 'valheim', component: Valheim}
  //{ path: '/:pathMatch(.*)*', component: NotFound },
];
const router = createRouter({

  history: createWebHistory(), // Используйте HTML5 History API
  routes // Передайте массив маршрутов
});
export default router;
createApp(App).use(router).mount('#app')
