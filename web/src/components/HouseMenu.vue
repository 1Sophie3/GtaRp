<template>
  <div>
    <!-- HAUS / VERWALTUNGSMENÜ -->
    <div v-if="isHouseMenuVisible" id="house-menu" class="house-container">
      <div class="header">
        <h2>{{ isManagementView ? 'Immobilienverwaltung' : houseData.name }}</h2>
        <button class="close-button" @click="closeAll">X</button>
      </div>

      <div class="content">
        <!-- Hauptansicht: Info -->
        <div v-if="!isManagementView" class="house-info">
          <p v-if="houseData.isForSale">
            Preis: <span>${{ formatNumber(houseData.price) }}</span>
          </p>
          <p v-else-if="houseData.isRentable && !houseData.isRenter">
            Miete: <span>${{ formatNumber(houseData.rentPrice) }} / Tag</span>
          </p>
          <p v-else>
            <template v-if="houseData.isRentable">
              Mieter: <span>{{ houseData.renterName }}</span>
            </template>
            <template v-else>
              Besitzer: <span>{{ houseData.ownerName }}</span>
            </template>
          </p>
        </div>

        <!-- Verwaltungsansicht: Info & Schlüsselbesitzer -->
        <div v-else class="house-info">
          <p>Verwalte deine Schlüssel und Schlösser.</p>
          
          <template v-if="houseData.KeyHolders && houseData.KeyHolders.length > 0">
            <p style="margin-top: 15px; font-weight: bold;">Schlüsselbesitzer:</p>
            <div 
              v-for="holder in houseData.KeyHolders" 
              :key="holder.TargetAccountId" 
              class="key-holder-entry"
            >
              <span>{{ holder.TargetPlayerName }} (ID: {{ holder.TargetAccountId }})</span>
              <button class="remove-key-btn" @click="removeKey(holder)">Entziehen</button>
            </div>
          </template>
        </div>

        <!-- Aktionen / Buttons -->
        <div class="menu-options">
          <!-- Szenario 1: Haus steht zum Verkauf -->
          <button v-if="!isManagementView && houseData.isForSale" class="menu-button" @click="sendAction('buy')">
            Haus kaufen
          </button>

          <!-- Szenario 2: Mieten -->
          <template v-if="!isManagementView && houseData.isRentable && !houseData.isRenter">
            <button class="menu-button" @click="sendAction('rent_1')">Für 1 Tag mieten</button>
            <button class="menu-button" @click="sendAction('rent_7')">Für 7 Tage mieten</button>
            <button class="menu-button" @click="sendAction('rent_30')">Für 30 Tage mieten</button>
          </template>

          <!-- Szenario 3: Interaktion für Bewohner / Besitzer -->
          <template v-if="!isManagementView && !houseData.isForSale && (!houseData.isRentable || houseData.isRenter)">
            <button 
              v-if="houseData.isOwner || houseData.isRenter || houseData.hasKey" 
              class="menu-button" 
              @click="sendAction('enter')"
            >
              Haus betreten
            </button>
            
            <button 
              v-if="houseData.isOwner || houseData.isRenter" 
              class="menu-button" 
              @click="sendAction('toggleLock')"
            >
              {{ houseData.isLocked ? 'Aufschließen' : 'Abschließen' }}
            </button>

            <button v-if="houseData.isOwner" class="menu-button" @click="isManagementView = true">
              Immobilie verwalten
            </button>
          </template>

          <!-- Optionen im Verwaltungsmenü -->
          <template v-if="isManagementView">
            <button class="menu-button" @click="openInputModal('Account-ID des Spielers', 'giveKey')">
              Schlüssel übergeben (2.500$)
            </button>
            <button class="menu-button sell" @click="sendAction('changeLocks')">
              Schlösser wechseln (25.000$)
            </button>
            <button class="menu-button" @click="isManagementView = false">
              Zurück zum Hauptmenü
            </button>
          </template>
        </div>
      </div>
    </div>

    <!-- INPUT MODAL (Ersatz für house.html) -->
    <div v-if="isInputModalVisible" class="input-modal-overlay">
      <div class="input-container">
        <h3>{{ inputTitle }}</h3>
        <input 
          v-model="inputValue" 
          type="text" 
          placeholder="Wert eingeben..." 
          @keydown.enter="submitInput"
          autofocus
        />
        <button class="menu-button" @click="submitInput">Bestätigen</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const isHouseMenuVisible = ref(false);
const isManagementView = ref(false);
const houseData = ref({});

const isInputModalVisible = ref(false);
const inputTitle = ref('');
const inputValue = ref('');
const currentInputType = ref('');

const formatNumber = (val) => {
  return val ? val.toLocaleString() : '0';
};

const sendNuiCallback = (eventName, data = {}) => {
  fetch(`https://${GetParentResourceName()}/${eventName}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json; charset=UTF-8' },
    body: JSON.stringify(data)
  }).catch(() => {});
};

const closeAll = () => {
  isHouseMenuVisible.value = false;
  isManagementView.value = false;
  isInputModalVisible.value = false;
  inputValue.value = '';
  sendNuiCallback('house:closeMenu');
};

const sendAction = (action, data = null) => {
  sendNuiCallback('house:action', { action, data });
  // Aktionen, die das Menü schließen
  if (!['manage', 'back'].includes(action)) {
    closeAll();
  }
};

const removeKey = (holder) => {
  if (confirm(`Möchtest du den Schlüssel von ${holder.TargetPlayerName} wirklich entziehen?`)) {
    sendAction('removeKey', holder.TargetAccountId);
  }
};

const openInputModal = (title, type) => {
  inputTitle.value = title;
  currentInputType.value = type;
  inputValue.value = '';
  isHouseMenuVisible.value = false;
  isInputModalVisible.value = true;
};

const submitInput = () => {
  if (inputValue.value.trim() !== '') {
    if (currentInputType.value === 'giveKey') {
      const targetId = parseInt(inputValue.value);
      if (!isNaN(targetId) && targetId > 0) {
        sendNuiCallback('house:giveKey', { targetId });
      }
    }
  }
  closeAll();
};

const handleKeyDown = (e) => {
  if (e.key === 'Escape' && (isHouseMenuVisible.value || isInputModalVisible.value)) {
    closeAll();
  }
};

const handleNuiMessage = (event) => {
  const { action, payload } = event.data;

  if (action === 'openHouseMenu') {
    houseData.value = payload;
    isManagementView.value = false;
    isHouseMenuVisible.value = true;
  } else if (action === 'closeHouseMenu') {
    closeAll();
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
@import url('https://fonts.googleapis.com/css2?family=Roboto:wght@400;500&display=swap');

* {
  box-sizing: border-box;
}

html, body {
  margin: 0;
  padding: 0;
  font-family: 'Arial', sans-serif;
  background: transparent;
  color: white;
  user-select: none;
}

/* Haupt-Container */
.house-container {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: rgba(30, 30, 30, 0.85);
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 0 20px rgba(0, 255, 255, 0.5);
  width: 400px;
  max-width: 90%;
  border: 1px solid #00ffff;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  border-bottom: 1px solid #444;
  padding-bottom: 10px;
}

.header h2 {
  color: #00ffff;
  margin: 0;
  text-shadow: 0 0 5px rgba(0, 255, 255, 0.7);
  font-size: 22px;
}

.close-button {
  background-color: #dc3545;
  color: white;
  border: none;
  padding: 5px 12px;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  font-weight: bold;
  transition: background-color 0.2s ease;
}

.close-button:hover {
  background-color: #c82333;
}

.house-info p {
  margin: 4px 0;
  font-size: 16px;
  background-color: rgba(0, 0, 0, 0.2);
  padding: 8px;
  border-radius: 4px;
}

.house-info span {
  color: #00ffff;
  font-weight: bold;
}

.menu-options {
  margin-top: 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.menu-button {
  background-color: #007bff;
  color: #fff;
  border: 1px solid #0056b3;
  padding: 12px;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  text-align: center;
  transition: background-color 0.2s ease, transform 0.1s ease;
}

.menu-button:hover {
  background-color: #0056b3;
  transform: scale(1.02);
}

.menu-button.sell {
  background-color: #c82333;
  border-color: #a01a26;
}

.menu-button.sell:hover {
  background-color: #a01a26;
}

.key-holder-entry {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: rgba(0, 0, 0, 0.3);
  padding: 10px;
  border-radius: 4px;
  font-size: 15px;
  margin-top: 5px;
}

.remove-key-btn {
  background-color: #dc3545;
  color: white;
  border: 1px solid #c82333;
  padding: 5px 10px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  font-weight: bold;
  transition: background-color 0.2s ease;
}

.remove-key-btn:hover {
  background-color: #a01a26;
}

/* Eingabefenster Overlay (house.html Stil) */
.input-modal-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: rgba(0, 0, 0, 0.4);
}

.input-container {
  background: rgba(20, 20, 20, 0.95);
  padding: 25px 30px;
  border-radius: 8px;
  color: white;
  text-align: center;
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 0 15px rgba(0,0,0,0.5);
  display: flex;
  flex-direction: column;
  align-items: center;
}

.input-container h3 {
  margin-top: 0;
  font-weight: 500;
  font-size: 18px;
}

.input-container input {
  padding: 12px;
  width: 240px;
  margin-bottom: 15px;
  border-radius: 4px;
  border: 1px solid #444;
  background: #1a1a1a;
  color: white;
  font-size: 16px;
}

.input-container input:focus {
  outline: none;
  border-color: #007bff;
}
</style>