<!-- src/web/src/components/DevControlPanel.vue -->
<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { isFiveM } from '../services/nui';

const router = useRouter();
const isFiveMEnv = isFiveM();

// Switch: Senden wir Testdaten mit oder simulieren wir "Server offline / keine Daten"?
const simulateNoData = ref(false);

// Vordefinierte Test-Payloads
const mockPayloads: Record<string, unknown> = {
  Bank: { cash: 15000, bankBalance: 350000, accountNumber: 'LS-88392', transactions: [] },
  Dealership: { vehicles: [{ model: 'T20', price: 2000000 }, { model: 'Zentorno', price: 750000 }] },
  Garage: { inGarageVehicles: [{ Id: 1, ModelName: 'Elegy', NumberPlate: 'DEV 1', Health: 1000 }], maxCapacity: 10 },
  House: { houseId: 12, name: 'Vinewood Villa', price: 850000 },
  Inventory: { items: [{ name: 'bread', label: 'Brot', count: 3 }] }
};

const openMenu = (routeName: string, path: string) => {
  const action = 'open' + routeName;
  
  // Wenn simulateNoData aktiv ist, senden wir KEINE Daten mit (null/empty)
  const payload = simulateNoData.value ? null : (mockPayloads[routeName] || {});

  window.postMessage({ action, payload }, '*');
  router.push(path);
};

const toggleTacho = () => {
  window.postMessage({ action: 'showTacho', payload: true }, '*');
};

const routes = router.getRoutes().filter(r => r.path !== '/');
</script>

<template>
  <div v-if="!isFiveMEnv" class="dev-panel">
    <h4>Dev Controller</h4>

    <!-- Data Simulation Mode Toggle -->
    <div class="toggle-container">
      <label>
        <input type="checkbox" v-model="simulateNoData" />
        Offline-Modus testen (Keine Daten)
      </label>
    </div>

    <hr />

    <div class="btn-group">
      <button 
        v-for="route in routes" 
        :key="route.path"
        @click="openMenu(String(route.name), route.path)"
      >
        {{ route.name }}
      </button>
    </div>

    <hr />

    <div class="btn-group">
      <button @click="toggleTacho">Tacho (HUD Toggle)</button>
    </div>

    <hr />

    <button class="close-btn" @click="router.push('/')">Home / Menüs Zu</button>
  </div>
</template>

<style scoped>
.dev-panel {
  position: fixed;
  bottom: 15px;
  left: 15px;
  background: rgba(10, 10, 10, 0.95);
  border: 1px solid #00ffff;
  padding: 12px;
  border-radius: 8px;
  z-index: 999999;
  width: 220px;
  color: #fff;
  font-family: sans-serif;
  box-shadow: 0 0 10px rgba(0, 255, 255, 0.2);
}

.dev-panel h4 {
  margin: 0 0 8px 0;
  font-size: 13px;
  color: #00ffff;
  text-align: center;
}

.toggle-container {
  font-size: 11px;
  color: #ffaa00;
  margin-bottom: 5px;
}

.toggle-container label {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}

.dev-panel hr {
  border: 0;
  border-top: 1px solid #333;
  margin: 8px 0;
}

.btn-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.dev-panel button {
  background: #222;
  color: #fff;
  border: 1px solid #444;
  padding: 5px 8px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  text-align: left;
}

.dev-panel button:hover {
  background: #00ffff;
  color: #000;
}

.dev-panel .close-btn {
  width: 100%;
  background: #dc3545;
  border: none;
  text-align: center;
  font-weight: bold;
}
</style>