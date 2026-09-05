<template>
  <div v-if="isVisible" class="prompt-container">
    <div class="header">
      <h2>ANGELN</h2>
    </div>
    <div class="content">
      <p>Du hast etwas gefangen! Möchtest du weiter angeln?</p>
      <p class="small-text">(Drücke 'E' für Ja oder warte 5 Sekunden)</p>
      <div class="button-group">
        <button id="btn-yes" @click="respond(true)">Ja</button>
        <button id="btn-no" @click="respond(false)">Nein</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const isVisible = ref(false);
let autoCloseTimer = null;

const sendNuiCallback = (eventName, data = {}) => {
  fetch(`https://${GetParentResourceName()}/${eventName}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json; charset=UTF-8' },
    body: JSON.stringify(data)
  }).catch(() => {});
};

const respond = (choice) => {
  if (autoCloseTimer) {
    clearTimeout(autoCloseTimer);
    autoCloseTimer = null;
  }
  isVisible.value = false;
  sendNuiCallback('fishing:promptResponse', { choice });
};

const handleNuiMessage = (event) => {
  const { action } = event.data;

  if (action === 'showContinuePrompt') {
    isVisible.value = true;
    
    // Auto-Close nach 5 Sekunden (sendet 'false')
    if (autoCloseTimer) clearTimeout(autoCloseTimer);
    autoCloseTimer = setTimeout(() => {
      if (isVisible.value) {
        respond(false);
      }
    }, 5000);
  } else if (action === 'hideContinuePrompt') {
    if (autoCloseTimer) clearTimeout(autoCloseTimer);
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
@import url('https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap');

* {
  box-sizing: border-box;
}

.prompt-container {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: rgba(30, 30, 30, 0.85);
  padding: 30px;
  border-radius: 8px;
  box-shadow: 0 0 20px rgba(0, 255, 255, 0.55);
  width: 450px;
  max-width: 90%;
  border: 1px solid #00ffff;
  color: #fff;
  text-align: center;
  font-family: 'Roboto', 'Arial', sans-serif;
}

.header {
  margin-bottom: 20px;
  border-bottom: 1px solid rgba(0, 255, 255, 0.4);
  padding-bottom: 15px;
}

.header h2 {
  color: #00ffff;
  margin: 0;
  text-shadow: 0 0 8px rgba(0, 255, 255, 0.8);
  font-size: 24px;
  letter-spacing: 2px;
  text-transform: uppercase;
}

.content p {
  font-size: 18px;
  margin: 15px 0;
}

.content .small-text {
  font-size: 14px;
  color: #ccc;
  margin-top: -10px;
  margin-bottom: 25px;
}

.button-group {
  display: flex;
  justify-content: center;
  gap: 20px;
}

.button-group button {
  color: #fff;
  border: none;
  padding: 12px 30px;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  font-weight: bold;
  transition: all 0.2s ease;
  text-transform: uppercase;
}

#btn-yes {
  background-color: #28a745;
  border: 1px solid #208a38;
}

#btn-yes:hover {
  background-color: #218838;
  box-shadow: 0 0 15px rgba(40, 167, 69, 0.7);
}

#btn-no {
  background-color: #dc3545;
  border: 1px solid #b32a38;
}

#btn-no:hover {
  background-color: #c82333;
  box-shadow: 0 0 15px rgba(220, 53, 69, 0.7);
}
</style>