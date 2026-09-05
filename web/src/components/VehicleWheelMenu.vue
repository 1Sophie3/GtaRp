<!-- Vorerst fertig  -->

<template>
  <div v-if="isVisible" class="wheel-overlay" @contextmenu.prevent="closeMenu">
    <div class="wheel-container">
      <div class="center-text">
        <span>{{ centerTitle || 'INTERAKTION' }}</span>
        <small v-if="pendingConfirm" class="confirm-hint">Klick zum Bestätigen!</small>
        <small v-else-if="hoveredAction" class="release-hint">Taste loslassen</small>
      </div>

      <div 
        v-for="(item, index) in items" 
        :key="index"
        :class="[
          'wheel-slice', 
          { 
            active: hoveredAction === item.Action,
            confirming: pendingConfirm === item.Action 
          }
        ]"
        :style="getSliceStyle(index, items.length)"
        @mouseenter="setHover(item.Action)"
        @mouseleave="setHover(null)"
        @click="handleClick(item)"
      >
        <span class="slice-label">{{ item.Label }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const isDevMode = import.meta.env.DEV && typeof window.GetParentResourceName !== 'function';

const isVisible = ref(isDevMode);
const centerTitle = ref(isDevMode ? 'PERSON' : '');
const hoveredAction = ref(null);
const pendingConfirm = ref(null);

const items = ref(isDevMode ? [
  { Label: 'Stabilisieren', Action: 'stabilize', RequireConfirm: true },
  { Label: 'Durchsuchen', Action: 'search', RequireConfirm: false },
  { Label: 'Fesseln', Action: 'cuff', RequireConfirm: false },
  { Label: 'Tragen', Action: 'carry', RequireConfirm: false }
] : []);

const sendNuiCallback = (eventName, data = {}) => {
  if (typeof window.GetParentResourceName === 'function') {
    fetch(`https://${window.GetParentResourceName()}/${eventName}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json; charset=UTF-8' },
      body: JSON.stringify(data)
    }).catch(() => {});
  } else {
    console.log(`[NUI Callback] ${eventName}:`, data);
  }
};

const setHover = (action) => {
  hoveredAction.value = action;
  // Wenn gehovered wird, aber die vorherige Bestätigung nicht mehr dazu passt, zurücksetzen
  if (pendingConfirm.value && pendingConfirm.value !== action) {
    pendingConfirm.value = null;
  }
  sendNuiCallback('wheel:hover', { action });
};

// Manueller Klick (funktioniert für normale Aktionen ODER Bestätigungen)
const handleClick = (item) => {
  if (item.RequireConfirm && pendingConfirm.value !== item.Action) {
    pendingConfirm.value = item.Action;
    return;
  }
  triggerAction(item);
};

// Ausführen & Schließen
const triggerAction = (item) => {
  sendNuiCallback('wheel:action', { action: item.Action, targetData: item.TargetData });
  closeMenu();
};

const closeMenu = () => {
  isVisible.value = false;
  hoveredAction.value = null;
  pendingConfirm.value = null;
  sendNuiCallback('wheel:close');
};

const getSliceStyle = (index, total) => {
  const angle = (360 / total) * index;
  return {
    transform: `rotate(${angle}deg) translate(110px) rotate(-${angle}deg)`
  };
};

const handleNuiMessage = (event) => {
  const { action, payload } = event.data;
  if (action === 'openWheel') {
    items.value = payload.items || [];
    centerTitle.value = payload.centerText || '';
    hoveredAction.value = null;
    pendingConfirm.value = null;
    isVisible.value = true;
  } else if (action === 'closeWheel') {
    closeMenu();
  }
};

// TASTEN-STEUERUNG (E loslassen & ESC)
const handleKeyUp = (e) => {
  if (!isVisible.value) return;

  // Key 'e' oder 'E' losgelassen
  if (e.key === 'e' || e.key === 'E') {
    if (hoveredAction.value) {
      const selectedItem = items.value.find(i => i.Action === hoveredAction.value);
      if (selectedItem) {
        // Falls Sicherheitsklick nötig ist, nicht direkt bei KeyUp ausführen!
        if (selectedItem.RequireConfirm) {
          pendingConfirm.value = selectedItem.Action;
        } else {
          triggerAction(selectedItem);
        }
      }
    } else {
      // Nichts gehovered -> Menü einfach schließen
      closeMenu();
    }
  }
};

const handleKeyDown = (e) => {
  if (e.key === 'Escape') closeMenu();
  if (isDevMode && e.key === 'F8') isVisible.value = !isVisible.value;
};

onMounted(() => {
  window.addEventListener('message', handleNuiMessage);
  window.addEventListener('keydown', handleKeyDown);
  window.addEventListener('keyup', handleKeyUp);
});

onUnmounted(() => {
  window.removeEventListener('message', handleNuiMessage);
  window.removeEventListener('keydown', handleKeyDown);
  window.removeEventListener('keyup', handleKeyUp);
});
</script>

<style scoped>
.wheel-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: rgba(0, 0, 0, 0.25);
  user-select: none;
}

.wheel-container {
  position: relative;
  width: 300px;
  height: 300px;
  border-radius: 50%;
  background: rgba(20, 20, 20, 0.75);
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  border: 2px solid rgba(0, 255, 255, 0.3);
}

.center-text {
  font-family: 'Arial', sans-serif;
  color: #00ffff;
  font-weight: bold;
  text-align: center;
  z-index: 10;
  font-size: 14px;
  text-transform: uppercase;
  pointer-events: none;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.confirm-hint {
  color: #ffc107;
  font-size: 10px;
  margin-top: 4px;
  animation: pulse 0.8s infinite alternate;
}

.release-hint {
  color: #a0a0a0;
  font-size: 10px;
  margin-top: 4px;
}

.wheel-slice {
  position: absolute;
  width: 85px;
  height: 85px;
  border-radius: 50%;
  background: rgba(35, 35, 35, 0.9);
  border: 1px solid #00ffff;
  display: flex;
  justify-content: center;
  align-items: center;
  color: #fff;
  cursor: pointer;
  transition: transform 0.2s, background 0.2s;
}

.wheel-slice:hover,
.wheel-slice.active {
  background: rgba(0, 255, 255, 0.4);
  transform: scale(1.1);
}

.wheel-slice.confirming {
  background: rgba(255, 193, 7, 0.6) !important;
  border-color: #ffc107 !important;
  transform: scale(1.15) !important;
  box-shadow: 0 0 15px rgba(255, 193, 7, 0.8);
}

.slice-label {
  font-size: 11px;
  font-weight: bold;
  text-align: center;
  padding: 5px;
}

@keyframes pulse {
  from { opacity: 0.6; }
  to { opacity: 1; }
}
</style>