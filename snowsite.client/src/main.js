
import { createApp } from 'vue'
import App from './App.vue'

import { createRouter, createWebHistory } from 'vue-router';
import Social from './SocialNetwork.vue';
import Main from './MainPage.vue';
import YandexMap from './YandexMap.vue';
import Examples from './Examples.vue';
import VideoCall from './VideoCall.vue';
import Valheim from './Valheim.vue';
import AuthPage from './AuthPage.vue'
import AppLayout from './components/AppLayout.vue';
import MySkills from './components/MySkills.vue';


const routes = [
  { path: '/', name: 'card', component: Main },
  { path: '/home', redirect: '/' },
  { path: '/chat', name: 'social', component: Social },
  { path: '/map', name: 'map', component: YandexMap },
  { path: '/examples', name: 'examples', component: Examples },
  { path: '/videocall', name: 'videocall', component: VideoCall },
  { path: '/valheim', name: 'valheim', component: Valheim },
  { path: '/myskills', name: 'skills', component: MySkills },
  { path: '/auth', component: AuthPage },
  {
    path: '/',
    component: Social,
    beforeEnter: (to, from, next) => {
      if (!localStorage.getItem('token')) {
        next('/auth');
      } else {
        next();
      }
    }
  },

  { path: '/uploads/:pathMatch(.*)*', redirect: () => window.location.href }

];
const router = createRouter({

  history: createWebHistory(), // Используйте HTML5 History API
  routes // Передайте массив маршрутов
});
export default router;
createApp(App).use(router).mount('#app')
