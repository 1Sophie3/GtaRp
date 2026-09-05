<!-- Bankmenü 
 Diese bankmenü verwaltet die ein- und auszahlungen, überweisungen und den kontoauszug der Spieler im RP

 Das Bankmenü ist in zwei Tabs unterteilt: "Transaktionen" und "Kontoauszug".
 Im Tab "Transaktionen" können Spieler Geld einzahlen, abheben und Überweisungen tätigen. Im Tab "Kontoauszug" können Spieler ihre letzten Transaktionen einsehen.
 Die Kommunikation mit dem FiveM-Server erfolgt über NUI-Callbacks, die über die Funktion sendNuiCallback(eventName, data) gesendet werden. Die Daten werden als JSON-Objekt an den Server übermittelt.
 Die Funktion handleNuiMessage(event) empfängt Nachrichten vom Server und aktualisiert die Kontodaten und Transaktionen entsprechend. Das Menü kann über die Aktionen "openBank" und "closeBank" geöffnet und geschlossen werden.
-->


<!-- Bankmenü mit Statusbanner & Fraktionskonto-Umschaltung -->
<template>
  <div v-if="isVisible" class="bank-container">
    <div class="header">
      <h2>Bankverwaltung</h2>
      <button class="close-button" @click="closeMenu">X</button>
    </div>

    <!-- Statusbanner / Konto-Umschaltung (wird angezeigt, wenn isLeader true ist) -->
    <div v-if="isLeader" class="account-type-banner">
      <span class="banner-label">Aktiver Kontotyp:</span>
      <div class="toggle-buttons">
        <button 
          class="banner-btn" 
          :class="{ active: selectedAccountType === 'private' }"
          @click="switchAccountType('private')"
        >
          Privatkonto
        </button>
        <button 
          class="banner-btn faction" 
          :class="{ active: selectedAccountType === 'faction' }"
          @click="switchAccountType('faction')"
        >
          Fraktionskonto ({{ factionName || 'Fraktion' }})
        </button>
      </div>
    </div>

    <div class="account-info">
      <p>Bargeld: <span>{{ formattedCash }}</span></p>
      <p>
        {{ selectedAccountType === 'faction' ? 'Fraktionsguthaben:' : 'Bankguthaben:' }} 
        <span :class="{ 'faction-balance': selectedAccountType === 'faction' }">{{ formattedBank }}</span>
      </p>
      <p>Kontonummer: <span>{{ accountNumber || 'N/A' }}</span></p>
    </div>

    <div class="tab-navigation">
      <button 
        class="tab-btn" 
        :class="{ active: activeTab === 'actions' }" 
        @click="activeTab = 'actions'"
      >
        Transaktionen
      </button>
      <button 
        class="tab-btn" 
        :class="{ active: activeTab === 'history' }" 
        @click="activeTab = 'history'"
      >
        Kontoauszug
      </button>
    </div>

    <div class="content">
      <!-- Tab: Transaktionen -->
      <div v-if="activeTab === 'actions'" class="tab-panel active">
        <div class="transaction-section">
          <h3>Ein- & Auszahlung</h3>
          <input 
            v-model.number="cashAmount" 
            type="number" 
            placeholder="Betrag" 
            min="1" 
            step="1"
          >
          <button @click="handleDeposit">Einzahlen</button>
          <button @click="handleWithdraw">Abheben</button>
        </div>

        <div class="transaction-section">
          <h3>Überweisung</h3>
          <input 
            v-model="transferAccount" 
            type="text" 
            placeholder="Empfänger-Kontonummer (9 Ziffern)"
          >
          <input 
            v-model.number="transferAmount" 
            type="number" 
            placeholder="Betrag" 
            min="1" 
            step="1"
          >
          <input 
            v-model="transferDescription" 
            type="text" 
            placeholder="Verwendungszweck (Erforderlich)" 
            maxlength="50"
          >
          <button @click="handleTransfer">Überweisen</button>
        </div>
      </div>

      <!-- Tab: Kontoauszug -->
      <div v-if="activeTab === 'history'" class="tab-panel active">
        <div id="transactionHistory" class="transaction-section">
          <h3>Letzte Transaktionen ({{ selectedAccountType === 'faction' ? 'Fraktion' : 'Privat' }})</h3>
          <ul id="transactionList">
            <li v-if="displayedTransactions.length === 0">Keine Transaktionen vorhanden.</li>
            <li v-for="(tx, index) in displayedTransactions" :key="index">
              <div class="transaction-details">
                <div v-if="tx.Type === 'deposit'">Einzahlung</div>
                <div v-else-if="tx.Type === 'withdraw'">Auszahlung</div>
                <div v-else-if="tx.Type === 'transfer_sent'">
                  Überweisung an {{ tx.TargetKontonummer }}:<br>
                  <small style="color: #ddd;">{{ tx.Description }}</small>
                </div>
                <div v-else-if="tx.Type === 'transfer_received'">
                  Überweisung von {{ tx.SourceKontonummer }}:<br>
                  <small style="color: #ddd;">{{ tx.Description }}</small>
                </div>
                <div v-else-if="tx.Type === 'system_charge'">
                  Systembelastung:<br>
                  <small style="color: #ddd;">{{ tx.Description }}</small>
                </div>
                <div v-else>
                  {{ tx.Description || 'Systemtransaktion' }}
                </div>

                <small style="color: #aaa;">{{ formatDate(tx.TransactionDate) }}</small>
              </div>
              <div 
                class="transaction-amount" 
                :class="tx.Amount > 0 ? 'amount-positive' : 'amount-negative'"
              >
                {{ tx.Amount > 0 ? '+' : '' }}{{ tx.Amount.toFixed(2) }}€
              </div>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';

const isVisible = ref(false);
const activeTab = ref('actions');

// Spieler- & Konto-Status
const isLeader = ref(false);
const factionName = ref('');
const selectedAccountType = ref('private'); // 'private' oder 'faction'

// Kontodaten
const cash = ref(0);
const privateBankBalance = ref(0);
const privateAccountNumber = ref('');
const privateTransactions = ref([]);

const factionBankBalance = ref(0);
const factionAccountNumber = ref('');
const factionTransactions = ref([]);

// Input-Formulardaten
const cashAmount = ref(null);
const transferAccount = ref('');
const transferAmount = ref(null);
const transferDescription = ref('');

// Dynamische Anzeige basierend auf ausgewähltem Kontotyp
const bankBalance = computed(() => selectedAccountType.value === 'faction' ? factionBankBalance.value : privateBankBalance.value);
const accountNumber = computed(() => selectedAccountType.value === 'faction' ? factionAccountNumber.value : privateAccountNumber.value);
const displayedTransactions = computed(() => selectedAccountType.value === 'faction' ? factionTransactions.value : privateTransactions.value);

const formattedCash = computed(() => `${cash.value}€`);
const formattedBank = computed(() => `${bankBalance.value}€`);

const formatDate = (dateString) => {
  if (!dateString) return '';
  return new Date(dateString).toLocaleString('de-DE');
};

const sendNuiCallback = (eventName, data = {}) => {
  fetch(`https://${GetParentResourceName()}/${eventName}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json; charset=UTF-8' },
    body: JSON.stringify(data)
  }).catch(() => {});
};

const switchAccountType = (type) => {
  selectedAccountType.value = type;
  sendNuiCallback('bank:switchAccountType', { accountType: type });
};

const closeMenu = () => {
  isVisible.value = false;
  sendNuiCallback('bank:close');
};

const handleDeposit = () => {
  if (!cashAmount.value || cashAmount.value <= 0) {
    alert('Bitte geben Sie einen gültigen Betrag ein.');
    return;
  }
  sendNuiCallback('bank:deposit', { 
    amount: cashAmount.value, 
    accountType: selectedAccountType.value 
  });
  cashAmount.value = null;
};

const handleWithdraw = () => {
  if (!cashAmount.value || cashAmount.value <= 0) {
    alert('Bitte geben Sie einen gültigen Betrag ein.');
    return;
  }
  sendNuiCallback('bank:withdraw', { 
    amount: cashAmount.value, 
    accountType: selectedAccountType.value 
  });
  cashAmount.value = null;
};

const handleTransfer = () => {
  const account = transferAccount.value.trim();
  const amount = transferAmount.value;
  const description = transferDescription.value.trim();

  if (!amount || amount <= 0) {
    alert('Bitte geben Sie einen gültigen Überweisungsbetrag ein.');
    return;
  }
  if (!account || account.length !== 9 || !/^\d+$/.test(account)) {
    alert('Bitte geben Sie eine gültige, 9-stellige Kontonummer ein.');
    return;
  }
  if (!description) {
    alert('Ein Verwendungszweck ist für die Überweisung erforderlich.');
    return;
  }

  sendNuiCallback('bank:transfer', {
    targetAccount: account,
    amount: amount,
    description: description,
    accountType: selectedAccountType.value
  });

  transferAccount.value = '';
  transferAmount.value = null;
  transferDescription.value = '';
};

const handleNuiMessage = (event) => {
  const { action, payload } = event.data;

  if (action === 'openBank') {
    cash.value = payload.cash || 0;
    
    // Führungsstatus & Fraktions-Infos
    isLeader.value = payload.isLeader || false;
    factionName.value = payload.factionName || '';
    selectedAccountType.value = 'private';

    // Daten Privatkonto
    privateBankBalance.value = payload.bankBalance || 0;
    privateAccountNumber.value = payload.accountNumber || '';
    privateTransactions.value = payload.transactions || [];

    // Daten Fraktionskonto (falls Leader)
    if (isLeader.value && payload.factionData) {
      factionBankBalance.value = payload.factionData.bankBalance || 0;
      factionAccountNumber.value = payload.factionData.accountNumber || '';
      factionTransactions.value = payload.factionData.transactions || [];
    }

    isVisible.value = true;
  } else if (action === 'closeBank') {
    isVisible.value = false;
  } else if (action === 'updateBalances') {
    cash.value = payload.cash;
    privateBankBalance.value = payload.bankBalance;
    if (payload.factionBankBalance !== undefined) {
      factionBankBalance.value = payload.factionBankBalance;
    }
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

.bank-container {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: rgba(30, 30, 30, 0.95);
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 0 15px rgba(0, 255, 255, 0.5);
  width: 650px;
  max-width: 90%;
  border: 1px solid #00ffff;
  display: flex;
  flex-direction: column;
  font-family: 'Arial', sans-serif;
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

/* Statusbanner Styling */
.account-type-banner {
  background: rgba(0, 255, 255, 0.08);
  border: 1px solid #00ffff;
  border-radius: 5px;
  padding: 10px 15px;
  margin-top: 15px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.banner-label {
  color: #00ffff;
  font-weight: bold;
  font-size: 14px;
}

.toggle-buttons {
  display: flex;
  gap: 10px;
}

.banner-btn {
  background: rgba(0, 0, 0, 0.4);
  border: 1px solid #555;
  color: #aaa;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 13px;
  transition: all 0.2s ease;
}

.banner-btn.active {
  background: #00ffff;
  color: #000;
  font-weight: bold;
  border-color: #00ffff;
  box-shadow: 0 0 8px rgba(0, 255, 255, 0.4);
}

.banner-btn.faction.active {
  background: #ffc107;
  border-color: #ffc107;
  color: #000;
  box-shadow: 0 0 8px rgba(255, 193, 7, 0.4);
}

.faction-balance {
  color: #ffc107;
}

.account-info {
  color: #fff;
  padding: 15px;
  background-color: rgba(0, 0, 0, 0.2);
  border-radius: 5px;
  margin-top: 15px;
  width: 100%;
}

.account-info p {
  font-size: 16px;
  margin: 8px 0;
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

.transaction-section input[type="number"],
.transaction-section input[type="text"] {
  width: 100%;
  padding: 10px;
  margin-bottom: 10px;
  border: 1px solid #555;
  border-radius: 4px;
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
  font-size: 14px;
}

.transaction-section input::placeholder {
  color: #aaa;
}

.transaction-section button {
  background-color: #007bff;
  color: #fff;
  border: none;
  padding: 10px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.2s ease;
  margin-right: 10px;
  width: 100px;
}

.transaction-section button:hover {
  background-color: #0056b3;
}

#transactionList {
  list-style-type: none;
  padding: 0;
  margin: 0;
  max-height: 300px;
  overflow-y: auto;
  padding-right: 5px;
}

#transactionList li {
  background-color: rgba(255, 255, 255, 0.05);
  padding: 12px;
  border-bottom: 1px solid #444;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
}

.transaction-details {
  flex-grow: 1;
}

.transaction-amount {
  font-weight: bold;
  min-width: 90px;
  text-align: right;
  font-size: 15px;
}

.amount-positive {
  color: #28a745;
}

.amount-negative {
  color: #dc3545;
}

#transactionList::-webkit-scrollbar {
  width: 8px;
}

#transactionList::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.2);
  border-radius: 4px;
}

#transactionList::-webkit-scrollbar-thumb {
  background-color: #00ffff;
  border-radius: 4px;
}
</style>