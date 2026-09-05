import { createRouter, createWebHashHistory, type RouteRecordRaw } from 'vue-router';

// Lazy-Loading-Pfade passend zu deiner src/components/ Ordnerstruktur
const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../components/CardsView.vue')
  },
  {
    path: '/CardsView',
    name: 'CardsView',
    component: () => import('../components/CardsView.vue')
  },
  {
    path: '/garage',
    name: 'Garage',
    component: () => import('../components/GarageMenu.vue')
  },
  {
    path: '/bank',
    name: 'Bank',
    component: () => import('../components/BankMenu.vue')
  },
  {
    path: '/dealership',
    name: 'Dealership',
    component: () => import('../components/DealershipMenu.vue')
  },
  {
    path: '/char-creator',
    name: 'CharCreator',
    component: () => import('../components/CharCreator.vue')
  },
  {
    path: '/admin',
    name: 'AdminPanel',
    component: () => import('../components/AdminPanel.vue')
  },
  {
    path: '/fraction',
    name: 'FractionMenu',
    component: () => import('../components/FractionMenu.vue')
  },
  {
    path: '/house',
    name: 'HouseMenu',
    component: () => import('../components/HouseMenu.vue')
  },
  {
    path: '/inventory',
    name: 'Inventory',
    component: () => import('../components/inventorySystem.vue')
  },
  {
    path: '/deathscreen',
    name: 'DeathScreen',
    component: () => import('../components/DeathScreen.vue')
  },
  {
    path: '/fishingcontroler',
    name: 'FishingControler',
    component: () => import('../components/FishingControler.vue')
  },
  {
    path: '/vehicleWheelmenu',
    name: 'VehicleWheelmenu',
    component: () => import('../components/VehicleWheelmenu.vue')
  }, 
];

const router = createRouter({
  history: createWebHashHistory(), // Zwingend notwendig für FiveM NUI
  routes
});

export default router;