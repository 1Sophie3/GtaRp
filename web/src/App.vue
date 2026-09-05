<!-- src/web/src/App.vue -->
<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import DevControlPanel from './components/DevControlPanel.vue';
import AppToast from './components/Notifications.vue';
import TachoGauge from './components/TachoGauge.vue';

const router = useRouter();
const isTachoVisible = ref(false);

const handleNuiMessage = (event: MessageEvent) => {
  const { action, payload } = event.data || {};
  if (!action) return;

  // 1. Tacho/HUD Overlays
  if (action === 'showTacho') {
    isTachoVisible.value = typeof payload === 'boolean' ? payload : !isTachoVisible.value;
    return;
  }

  // 2. Schließen-Event
  if (action === 'closeAll') {
    router.push('/');
    return;
  }

  // 3. Dynamisches Routing: Überprüft automatisch alle registrierten Routen
  // wandelt z. B. 'openGarage' -> '/garage' oder 'openDealership' -> '/dealership' um
  const targetPath = '/' + action.replace(/^open/, '').toLowerCase();
  const availableRoutes = router.getRoutes();
  const routeExists = availableRoutes.some(route => route.path === targetPath);

  if (routeExists) {
    router.push(targetPath);
  }
};

onMounted(() => {
  window.addEventListener('message', handleNuiMessage);
});

onUnmounted(() => {
  window.removeEventListener('message', handleNuiMessage);
});
</script>

<template>
  <div id="nui-root">
    <RouterView v-slot="{ Component }">
      <KeepAlive :max="5">
        <component :is="Component" />
      </KeepAlive>
    </RouterView>

    <TachoGauge v-if="isTachoVisible" />
    <AppToast />
    <DevControlPanel />
  </div>
</template>

<style>
#nui-root {
  width: 100vw;
  height: 100vh;
  overflow: hidden;
  position: relative;
}
</style>