<template>
  <div v-if="isVisible" id="tacho-container">
    <svg id="gauge" viewBox="0 0 200 120">
      <path class="bg-arc" d="M 20 100 A 80 80 0 0 1 180 100"></path>
      <path 
        class="rpm-arc" 
        id="rpm-arc" 
        d="M 20 100 A 80 80 0 0 1 180 100" 
        :style="{ strokeDashoffset: rpmDashOffset }"
      ></path>
      <line 
        class="needle" 
        id="needle" 
        x1="100" y1="100" x2="100" y2="30" 
        :style="{ transform: `rotate(${needleAngle}deg)` }"
      ></line>
      <circle class="needle-base" cx="100" cy="100" r="5"></circle>
      <text class="speed-text" id="speed-text" x="100" y="85">{{ Math.floor(tachoData.Speed) }}</text>
      <text class="unit-text" x="100" y="100">KM/H</text>
      <text class="gear-text" id="gear-text" x="100" y="118">{{ gearDisplay }}</text>
    </svg>
    
    <div id="info-bars">
      <div class="bar-container">
        <span class="bar-label">FUEL</span>
        <div class="bar-bg">
          <div class="bar-fill" :style="{ width: `${tachoData.Fuel}%` }"></div>
        </div>
      </div>
      <div class="bar-container">
        <span class="bar-label">KM-STAND</span>
        <span id="mileage-text">{{ tachoData.Mileage.toFixed(1) }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';

const isVisible = ref(false);
const MAX_SPEED = 340;
const RPM_ARC_LENGTH = 251.327; // Bogenlänge für R80 (PI * 80)

const tachoData = ref({
  Speed: 0,
  Gear: 0,
  Rpm: 0,
  Fuel: 100,
  Mileage: 0.0
});

const gearDisplay = computed(() => {
  return tachoData.value.Gear > 0 ? tachoData.value.Gear : 'N';
});

const needleAngle = computed(() => {
  const speedPercent = Math.min(tachoData.value.Speed / MAX_SPEED, 1);
  return -90 + (speedPercent * 180);
});

const rpmDashOffset = computed(() => {
  return RPM_ARC_LENGTH - (tachoData.value.Rpm * RPM_ARC_LENGTH);
});

const handleNuiMessage = (event) => {
  const { action, payload } = event.data;
  if (action === 'showTacho') {
    isVisible.value = payload;
  } else if (action === 'updateTacho') {
    tachoData.value = payload;
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
#tacho-container {
  position: absolute;
  bottom: 20px;
  right: 20px;
  width: 220px;
  background: rgba(0, 0, 0, 0.5);
  border-radius: 10px;
  padding: 10px;
  color: white;
  font-family: 'Arial', sans-serif;
  user-select: none;
}

#gauge {
  width: 100%;
  height: auto;
}

.bg-arc {
  fill: none;
  stroke: #333;
  stroke-width: 8;
}

.rpm-arc {
  fill: none;
  stroke: #00ffff;
  stroke-width: 8;
  stroke-dasharray: 251.327;
  transition: stroke-dashoffset 0.1s linear;
}

.needle {
  stroke: #ff0000;
  stroke-width: 3;
  transform-origin: 100px 100px;
  transition: transform 0.1s ease-out;
}

.needle-base {
  fill: #fff;
}

.speed-text {
  fill: #fff;
  font-size: 28px;
  font-weight: bold;
  text-anchor: middle;
}

.unit-text {
  fill: #aaa;
  font-size: 10px;
  text-anchor: middle;
}

.gear-text {
  fill: #00ff00;
  font-size: 16px;
  font-weight: bold;
  text-anchor: middle;
}

#info-bars {
  margin-top: 5px;
}

.bar-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 11px;
  margin-bottom: 4px;
}

.bar-bg {
  width: 110px;
  height: 8px;
  background: #333;
  border-radius: 4px;
  overflow: hidden;
}

.bar-fill {
  height: 100%;
  background: #00ff8c;
  transition: width 0.2s ease;
}
</style>