<template>
  <div v-if="cardType" class="card-container">
    <!-- Personalausweis -->
    <div v-if="cardType === 'idcard'" class="card id-card">
      <div class="header">
        <h2>State of San Andreas</h2>
      </div>

      <div class="content">
        <span class="label">Nachname</span>
        <span class="data">{{ idData.lastname }}</span>

        <span class="label">Vorname</span>
        <span class="data">{{ idData.firstname }}</span>

        <span class="label">Geburtsdatum</span>
        <span class="data">{{ idData.birthdate }}</span>

        <span class="label">Geschlecht</span>
        <span class="data">{{ idData.gender }}</span>

        <span class="label">ID-Nummer</span>
        <span class="data">{{ formattedAccountId }}</span>

        <span class="label">Ausgestellt am</span>
        <span class="data">{{ idData.creationDate }}</span>
      </div>

      <div class="footer" v-if="idData.firstname && idData.lastname">
        <div class="signature-field">
          <div class="signature">{{ idData.firstname }} {{ idData.lastname }}</div>
          <div class="label">Unterschrift des Inhabers</div>
        </div>
      </div>
    </div>

    <!-- Führerschein -->
    <div v-if="cardType === 'license'" class="card license-card">
      <div class="header">
        <h2>FÜHRERSCHEIN</h2>
        <div class="owner-info">
          <div class="owner-name">Inhaber: {{ licenseData.ownerName }}</div>
          <div class="owner-id">ID: {{ formattedLicenseId }}</div>
        </div>
      </div>

      <div class="content">
        <div class="license-row">
          <div class="icon"><i class="fa-solid fa-car"></i></div>
          <div class="label">PKW-Klasse</div>
          <div class="license-date">{{ licenseData.carData || '---' }}</div>
          <div class="status" :class="licenseData.carData ? 'valid' : 'invalid'">
            {{ licenseData.carData ? '✔' : '✖' }}
          </div>
        </div>

        <div class="license-row">
          <div class="icon"><i class="fa-solid fa-truck"></i></div>
          <div class="label">LKW-Klasse</div>
          <div class="license-date">{{ licenseData.truckData || '---' }}</div>
          <div class="status" :class="licenseData.truckData ? 'valid' : 'invalid'">
            {{ licenseData.truckData ? '✔' : '✖' }}
          </div>
        </div>

        <div class="license-row">
          <div class="icon"><i class="fa-solid fa-ship"></i></div>
          <div class="label">Bootsklasse</div>
          <div class="license-date">{{ licenseData.boatData || '---' }}</div>
          <div class="status" :class="licenseData.boatData ? 'valid' : 'invalid'">
            {{ licenseData.boatData ? '✔' : '✖' }}
          </div>
        </div>

        <div class="license-row">
          <div class="icon"><i class="fa-solid fa-plane"></i></div>
          <div class="label">Flugzeugklasse</div>
          <div class="license-date">{{ licenseData.aircraftData || '---' }}</div>
          <div class="status" :class="licenseData.aircraftData ? 'valid' : 'invalid'">
            {{ licenseData.aircraftData ? '✔' : '✖' }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';

const cardType = ref(null); // 'idcard' | 'license' | null
const hideTimer = ref(null);

const idData = ref({
  firstname: '',
  lastname: '',
  birthdate: '',
  gender: '',
  accountId: null,
  creationDate: ''
});

const licenseData = ref({
  ownerName: '',
  ownerId: null,
  carData: null,
  truckData: null,
  boatData: null,
  aircraftData: null
});

// Formatiert Account-IDs wie z.B. SA-00000123
const formattedAccountId = computed(() => {
  return idData.value.accountId 
    ? `SA-${idData.value.accountId.toString().padStart(8, '0')}` 
    : 'N/A';
});

const formattedLicenseId = computed(() => {
  return licenseData.value.ownerId 
    ? `SA-${licenseData.value.ownerId.toString().padStart(8, '0')}` 
    : 'N/A';
});

// Steuerung der 5-Sekunden-Anzeige
const triggerAutoClose = () => {
  if (hideTimer.value) clearTimeout(hideTimer.value);
  hideTimer.value = setTimeout(() => {
    cardType.value = null;
  }, 5000);
};

const handleNuiMessage = (event) => {
  const { action, data } = event.data;

  if (action === 'showIdCard') {
    idData.value = data;
    cardType.value = 'idcard';
    triggerAutoClose();
  } else if (action === 'showLicense') {
    licenseData.value = data;
    cardType.value = 'license';
    triggerAutoClose();
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
@import url('https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&family=Sacramento&display=swap');
@import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.2/css/all.min.css');

.card-container {
  position: absolute;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  pointer-events: none;
}

.card {
  position: fixed;
  bottom: 25px;
  right: 25px;
  width: 280px;
  background-color: rgba(15, 25, 35, 0.95);
  border: 1px solid rgba(0, 255, 255, 0.7);
  border-radius: 8px;
  box-shadow: 0 0 15px rgba(0, 255, 255, 0.5);
  color: #ffffff;
  background-image: 
    repeating-linear-gradient(45deg, rgba(0, 255, 255, 0.05), rgba(0, 255, 255, 0.05) 1px, transparent 1px, transparent 10px),
    radial-gradient(ellipse at bottom right, rgba(80, 0, 160, 0.2), transparent 70%);
  display: flex;
  flex-direction: column;
  padding: 10px;
  font-family: 'Roboto', sans-serif;
}

.header {
  text-align: center;
  border-bottom: 1px solid rgba(0, 255, 255, 0.3);
  padding-bottom: 5px;
  margin-bottom: 10px;
}

.header h2 {
  margin: 0;
  font-size: 13px;
  font-weight: 700;
  color: #00ffff;
  text-shadow: 0 0 4px rgba(0, 255, 255, 0.7);
  letter-spacing: 1px;
  text-transform: uppercase;
}

/* Personalausweis Styling */
.id-card .content {
  display: grid;
  grid-template-columns: 90px 1fr;
  gap: 3px 8px;
  font-size: 11px;
}

.id-card .content .label {
  color: #88a1b9;
  text-align: right;
}

.id-card .content .data {
  font-weight: 700;
}

.id-card .footer {
  margin-top: 10px;
}

.signature-field .label {
  font-size: 9px;
  color: #88a1b9;
  text-align: center;
}

.signature {
  font-family: 'Sacramento', cursive;
  font-size: 24px;
  color: #00ffff;
  text-shadow: 0 0 4px rgba(0, 255, 255, 0.7);
  border-bottom: 1px solid rgba(0, 255, 255, 0.3);
  line-height: 1;
  text-align: center;
}

/* Führerschein Styling */
.owner-info { display: flex; justify-content: space-between; align-items: baseline; margin-top: 2px; }
.owner-name { font-size: 11px; color: #ccc; }
.owner-id { font-size: 9px; color: #88a1b9; }

.license-row {
  display: flex;
  align-items: center;
  margin-bottom: 8px;
  font-size: 14px;
}

.license-row .icon { width: 30px; text-align: center; color: #00ffff; }
.license-row .label { flex-grow: 1; padding-left: 10px; font-size: 12px; }
.license-date { font-size: 11px; color: #ccc; min-width: 70px; text-align: right; }
.license-row .status { font-size: 16px; font-weight: bold; width: 20px; text-align: center; }

.status.valid { color: #00ff6a; text-shadow: 0 0 5px #00ff6a; }
.status.invalid { color: #ff2d2d; text-shadow: 0 0 5px #ff2d2d; }
</style>