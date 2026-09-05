<template>
  <div v-if="isVisible" id="faction-menu" class="bank-container">
    <!-- Header -->
    <div class="header">
      <h2 id="faction-title">{{ factionName }} Dienstmenü</h2>
      <button class="close-button" id="close-btn" @click="closeMenu">X</button>
    </div>

    <!-- Tab Navigation -->
    <div class="tab-navigation">
      <button 
        class="tab-btn" 
        :class="{ active: activeTab === 'dienstPanel' }" 
        @click="activeTab = 'dienstPanel'"
      >
        Dienst
      </button>
      <button 
        class="tab-btn" 
        :class="{ active: activeTab === 'shopPanel' }" 
        @click="activeTab = 'shopPanel'"
      >
        Fraktionsladen
      </button>
    </div>

    <!-- Content -->
    <div class="content">
      <!-- Dienst Panel -->
      <div v-if="activeTab === 'dienstPanel'" id="dienstPanel" class="tab-panel active">
        <div class="transaction-section">
          <h3>Dienst-Verwaltung</h3>
          <p>Verwalte hier deinen aktuellen Dienst-Status.</p>
          <button v-if="!isOnDuty" id="startDutyBtn" @click="startDuty">Dienst Starten</button>
          <button v-if="isOnDuty" id="endDutyBtn" @click="endDuty">Dienst Beenden</button>
        </div>
      </div>

      <!-- Shop Panel -->
      <div v-if="activeTab === 'shopPanel'" id="shopPanel" class="tab-panel active">
        <div class="transaction-section">
          <h3>Fraktionsladen</h3>
          <p>Dieser Bereich ist derzeit außer Betrieb. Hier wirst du bald fraktionsspezifische Ausrüstung kaufen können.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const isVisible = ref(false);
const activeTab = ref('dienstPanel');
const factionName = ref('');
const isOnDuty = ref(false);

const sendNuiCallback = (eventName, data = {}) => {
  fetch(`https://${GetParentResourceName()}/${eventName}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json; charset=UTF-8' },
    body: JSON.stringify(data)
  }).catch(() => {});
};

const closeMenu = () => {
  isVisible.value = false;
  sendNuiCallback('faction:closeMenu');
};

const startDuty = () => {
  sendNuiCallback('faction:startDuty');
  closeMenu();
};

const endDuty = () => {
  sendNuiCallback('faction:endDuty');
  closeMenu();
};

const handleKeyDown = (e) => {
  if (e.key === 'Escape' && isVisible.value) {
    closeMenu();
  }
};

const handleNuiMessage = (event) => {
  const { action, payload } = event.data;

  if (action === 'openFactionMenu') {
    factionName.value = payload.factionName || 'Fraktion';
    isOnDuty.value = payload.isOnDuty || false;
    activeTab.value = 'dienstPanel';
    isVisible.value = true;
  } else if (action === 'closeFactionMenu') {
    isVisible.value = false;
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
* {
  box-sizing: border-box;
}

html, body {
  margin: 0;
  padding: 0;
  font-family: 'Arial', sans-serif;
  background: transparent;
  user-select: none;
}

.bank-container {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: rgba(30, 30, 30, 0.9);
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 0 15px rgba(0, 255, 255, 0.5);
  width: 650px; 
  max-width: 90%;
  border: 1px solid #00ffff;
  display: flex;
  flex-direction: column; 
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 15px;
  border-bottom: 1px solid #555;
  width: 100%;
}

.header h2 {
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
  margin-left: 20px; 
}

.close-button:hover {
  background-color: #c82333;
}

.tab-navigation {
  display: flex;
  margin-top: 20px;
  width: 100%;
}

.tab-btn {
  flex-grow: 1;
  padding: 12px;
  cursor: pointer;
  background: rgba(0, 0, 0, 0.2);
  border: 1px solid #444;
  border-bottom: none;
  color: #ccc;
  font-size: 16px;
  transition: background-color 0.3s, color 0.3s;
  border-radius: 5px 5px 0 0;
}

.tab-btn:not(:last-child) {
  margin-right: 5px;
}

.tab-btn:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.tab-btn.active {
  color: #00ffff;
  background-color: rgba(0, 0, 0, 0.4);
  border-color: #00ffff;
}

.content {
  color: #fff;
  width: 100%;
  padding: 20px;
  background-color: rgba(0, 0, 0, 0.4);
  border: 1px solid #00ffff;
  border-top: none;
  border-radius: 0 0 5px 5px;
}

.tab-panel {
  display: none;
}

.tab-panel.active {
  display: block;
}

.transaction-section {
  padding: 10px;
}

.transaction-section:not(:last-child) {
  margin-bottom: 20px;
}

.transaction-section h3 {
  margin-top: 0;
  margin-bottom: 15px;
  color: #00ffff;
  font-weight: bold;
  font-size: 18px;
  border-bottom: 1px solid #555;
  padding-bottom: 5px;
}

.transaction-section p {
  color: #ddd;
  line-height: 1.5;
}

.transaction-section button {
  background-color: #007bff;
  color: #fff;
  border: none;
  padding: 12px 18px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 16px;
  font-weight: bold;
  transition: background-color 0.2s ease;
  margin-right: 10px;
  min-width: 150px;
}

.transaction-section button:hover {
  background-color: #0056b3;
}

#endDutyBtn {
  background-color: #28a745;
}

#endDutyBtn:hover {
  background-color: #218838;
}
</style>