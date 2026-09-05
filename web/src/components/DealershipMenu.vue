<!-- Fertig  -->

<template>
  <div v-if="isVisible" class="container">
    <div class="header">
      <h2 id="vehicle-name">{{ vehicleInfo.DisplayName || 'Fahrzeugname' }}</h2>
      <button class="close-button" @click="closeMenu">X</button>
    </div>
    
    <div class="content">
      <p id="vehicle-description">
        {{ vehicleInfo.Description || 'Hier steht eine tolle Beschreibung des Fahrzeugs, die alle Features hervorhebt.' }}
      </p>
      <div class="info-bar">
        <span class="price-tag">
          Preis: <strong id="vehicle-price">${{ formatPrice(vehicleInfo.Price) }}</strong>
        </span>
      </div>
    </div>

    <div class="buttons">
      <button id="buy-btn" @click="buyVehicle(false)">Privat kaufen</button>
      <button 
        v-if="vehicleInfo.IsFactionBuyable && vehicleInfo.PlayerFactionRank >= 10" 
        id="buy-faction-btn" 
        @click="buyVehicle(true)"
      >
        Für Fraktion kaufen
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

interface VehicleInfo {
  DisplayName: string;
  Price: number;
  Description: string;
  Model: string;
  IsFactionBuyable: boolean;
  PlayerFactionRank: number;
}

const isVisible = ref(false);
const vehicleInfo = ref<VehicleInfo>({
  DisplayName: '',
  Price: 0,
  Description: '',
  Model: '',
  IsFactionBuyable: false,
  PlayerFactionRank: 0
});

const formatPrice = (price: number): string => {
  return price ? price.toLocaleString('de-DE') : '0';
};

// Globale Type-Deklaration für FiveM NUI
declare function GetParentResourceName(): string;

const sendNuiCallback = (eventName: string, data: Record<string, unknown> = {}) => {
  const resourceName = typeof GetParentResourceName === 'function' ? GetParentResourceName() : 'nui-mock';
  fetch(`https://${resourceName}/${eventName}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json; charset=UTF-8' },
    body: JSON.stringify(data)
  }).catch(() => {});
};

const closeMenu = () => {
  isVisible.value = false;
  sendNuiCallback('dealership:closeMenu');
};

const buyVehicle = (forFaction: boolean) => {
  sendNuiCallback('dealership:buy', { forFaction });
};

const handleNuiMessage = (event: MessageEvent) => {
  const { action, payload } = event.data || {};

  if (action === 'openDealership' && payload) {
    vehicleInfo.value = { ...vehicleInfo.value, ...payload };
    isVisible.value = true;
  } else if (action === 'closeDealership') {
    isVisible.value = false;
  }
};

onMounted(() => {
  window.addEventListener('message', handleNuiMessage);
});

onUnmounted(() => {
  window.removeEventListener('message', handleNuiMessage);
});
</script>

<style scoped>
* {
  box-sizing: border-box;
}

.container {
  background: rgba(20, 25, 30, 0.85);
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 0 20px rgba(0, 255, 255, 0.4);
  width: 550px;
  border: 1px solid rgba(0, 255, 255, 0.6);
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: #f0f0f0;
  user-select: none;
  font-family: 'Arial', sans-serif;
  z-index: 1000;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  border-bottom: 1px solid rgba(0, 255, 255, 0.3);
  padding-bottom: 15px;
}

.header h2 {
  color: #00ffff;
  margin: 0;
  text-shadow: 0 0 6px rgba(0, 255, 255, 0.7);
  font-size: 26px;
  font-weight: bold;
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

.content p {
  font-size: 16px;
  line-height: 1.6;
  margin-bottom: 20px;
  min-height: 50px;
}

.info-bar {
  text-align: center;
  margin: 25px 0;
}

.price-tag {
  font-size: 20px;
  color: #fff;
}

.price-tag strong {
  color: #00ff8c;
  font-weight: bold;
}

.buttons {
  display: flex;
  justify-content: space-around;
  margin-top: 15px;
}

.buttons button {
  background-color: #007bff;
  color: #fff;
  border: 1px solid #0056b3;
  padding: 12px 25px;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  font-weight: bold;
  transition: all 0.2s ease;
  flex-grow: 1;
  margin: 0 10px;
}

.buttons button:hover {
  background-color: #0056b3;
  box-shadow: 0 0 10px rgba(0, 123, 255, 0.5);
}

#buy-faction-btn {
  background-color: #17a2b8;
  border-color: #10707f;
}

#buy-faction-btn:hover {
  background-color: #10707f;
  box-shadow: 0 0 10px rgba(23, 162, 184, 0.5);
}
</style>