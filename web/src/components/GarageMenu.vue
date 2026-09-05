<template>
  <div v-if="isVisible" class="garage-container">
    <div class="header">
      <h2>Garagen Verwaltung</h2>
      <button class="close-button" @click="closeGarageUI">X</button>
    </div>

    <div class="tab-buttons">
      <button 
        :class="['tab-button', { active: activeTab === 'spawnTab' }]" 
        @click="activeTab = 'spawnTab'"
      >
        Fahrzeuge Spawnen
      </button>
      <button 
        :class="['tab-button', { active: activeTab === 'storeTab' }]" 
        @click="activeTab = 'storeTab'"
      >
        Fahrzeuge Einlagern
      </button>
    </div>

    <!-- Spawn Tab -->
    <div v-if="activeTab === 'spawnTab'" class="tab-content active">
      <h3>Deine Fahrzeuge in der Garage (<span>{{ inGarageVehicles.length }}</span>/<span>{{ maxCapacity }}</span>)</h3>
      <div class="vehicle-list">
        <p v-if="inGarageVehicles.length === 0" class="empty-text">
          Du hast keine Fahrzeuge in dieser Garage.
        </p>
        <div v-else v-for="veh in inGarageVehicles" :key="veh.Id" class="vehicle-item">
          <div class="vehicle-details">
            <p class="model-name">{{ veh.ModelName }}</p>
            <p class="plate">Kennzeichen: {{ veh.NumberPlate }}</p>
            <p>Gesundheit: {{ Math.round(veh.Health / 10) }}%</p>
          </div>
          <div class="vehicle-actions">
            <button @click="requestSpawnVehicle(veh.Id)">Spawnen</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Einlagern Tab -->
    <div v-if="activeTab === 'storeTab'" class="tab-content active">
      <h3>Einlagerbare Fahrzeuge in deiner Nähe</h3>
      <div class="vehicle-list">
        <p v-if="nearbyParkableVehicles.length === 0" class="empty-text">
          Keine einlagerbaren Fahrzeuge in deiner Nähe.
        </p>
        <div v-else v-for="veh in nearbyParkableVehicles" :key="veh.Id" class="vehicle-item">
          <div class="vehicle-details">
            <p class="model-name">{{ veh.ModelName }}</p>
            <p class="plate">Kennzeichen: {{ veh.NumberPlate }}</p>
            <p>Zustand: {{ Math.round(veh.Health / 10) }}%</p>
          </div>
          <div class="vehicle-actions">
            <button class="red-button" @click="requestStoreVehicle(veh.Id)">Einlagern</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

interface Vehicle {
  Id: number;
  ModelName: string;
  NumberPlate: string;
  Health: number;
}

declare function GetParentResourceName(): string;

const isVisible = ref(false);
const activeTab = ref<'spawnTab' | 'storeTab'>('spawnTab');
const inGarageVehicles = ref<Vehicle[]>([]);
const nearbyParkableVehicles = ref<Vehicle[]>([]);
const maxCapacity = ref(0);

const sendNuiCallback = (eventName: string, data: Record<string, unknown> = {}) => {
  const resourceName = typeof GetParentResourceName === 'function' ? GetParentResourceName() : 'nui-mock';
  fetch(`https://${resourceName}/${eventName}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json; charset=UTF-8' },
    body: JSON.stringify(data)
  }).catch(() => {});
};

const closeGarageUI = () => {
  isVisible.value = false;
  sendNuiCallback('garage:close');
};

const requestSpawnVehicle = (vehId: number) => {
  sendNuiCallback('garage:spawn', { vehId });
};

const requestStoreVehicle = (vehId: number) => {
  sendNuiCallback('garage:store', { vehId });
};

const handleNuiMessage = (event: MessageEvent) => {
  const { action, payload } = event.data || {};
  if (action === 'openGarage') {
    inGarageVehicles.value = payload?.inGarageVehicles || [];
    nearbyParkableVehicles.value = payload?.nearbyParkableVehicles || [];
    maxCapacity.value = payload?.maxCapacity || 0;
    activeTab.value = 'spawnTab';
    isVisible.value = true;
  } else if (action === 'closeGarage') {
    isVisible.value = false;
  }
};

const handleKeyDown = (e: KeyboardEvent) => {
  if (e.key === 'Escape' && isVisible.value) {
    closeGarageUI();
  }
};

onMounted(() => {
  window.addEventListener('message', handleNuiMessage);
  window.addEventListener('keydown', handleKeyDown);
});

onUnmounted(() => {
  window.removeEventListener('message', handleNuiMessage);
  window.removeEventListener('keydown', handleKeyDown);
});
</script>

<style scoped>
.garage-container {
  background-color: rgba(30, 30, 30, 0.9);
  padding: 30px;
  border-radius: 8px;
  box-shadow: 0 0 15px rgba(0, 255, 255, 0.5);
  width: 600px;
  max-width: 90%;
  border: 1px solid #00ffff;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: #fff;
  font-family: 'Arial', sans-serif;
  font-size: 14px;
  z-index: 1000;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
  border-bottom: 1px solid #555;
  padding-bottom: 10px;
}

h2 {
  color: #00ffff;
  margin: 0;
  text-shadow: 0 0 5px rgba(0, 255, 255, 0.7);
  font-size: 24px;
}

.close-button {
  background-color: #dc3545;
  color: white;
  border: none;
  padding: 8px 15px;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  transition: background-color 0.2s ease;
}

.close-button:hover {
  background-color: #c82333;
}

.tab-buttons {
  display: flex;
  justify-content: center;
  margin-bottom: 20px;
}

.tab-button {
  background-color: #333;
  color: #fff;
  border: none;
  padding: 10px 20px;
  cursor: pointer;
  font-size: 16px;
  transition: background-color 0.2s ease, color 0.2s ease;
  border-radius: 5px 5px 0 0;
  margin: 0 5px;
}

.tab-button:hover {
  background-color: #555;
}

.tab-button.active {
  background-color: #00ffff;
  color: #000;
  font-weight: bold;
  box-shadow: 0 0 8px rgba(0, 255, 255, 0.7);
}

.tab-content {
  padding: 15px 0;
}

.vehicle-list {
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid #444;
  padding: 10px;
  border-radius: 5px;
  background-color: rgba(0, 0, 0, 0.3);
}

.empty-text {
  text-align: center;
  color: #ccc;
  padding: 20px;
  margin: 0;
}

.vehicle-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px;
  margin-bottom: 8px;
  background-color: #2a2a2a;
  border: 1px solid #555;
  border-radius: 4px;
}

.vehicle-item:hover {
  background-color: #3a3a3a;
}

.vehicle-details {
  flex-grow: 1;
}

.vehicle-details p {
  margin: 2px 0;
}

.vehicle-details .model-name {
  font-weight: bold;
  color: #00ff00;
}

.vehicle-details .plate {
  color: #ccc;
  font-size: 0.9em;
}

.vehicle-actions button {
  background-color: #007bff;
  color: white;
  border: none;
  padding: 8px 15px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.2s ease;
  margin-left: 10px;
}

.vehicle-actions button:hover {
  background-color: #0056b3;
}

.vehicle-actions button.red-button {
  background-color: #dc3545;
}

.vehicle-actions button.red-button:hover {
  background-color: #c82333;
}
</style>