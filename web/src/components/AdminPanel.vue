<!-- Admin Panel
Erstmal fertig  -->


<template>
  <div v-if="isVisible" class="container">
    <!-- Header -->
    <div class="header">
      <h2><i class="fas fa-user-shield"></i> Admin Panel</h2>
      <button class="close-button" @click="closePanel">×</button>
    </div>

    <div class="main-body">
      <!-- Tabs Sidebar -->
      <div class="tabs">
        <button 
          class="tab-button" 
          :class="{ active: activeTab === 'players' }" 
          @click="activeTab = 'players'"
        >
          <i class="fas fa-users"></i> Spieler
        </button>
        <button 
          v-if="adminLevel >= 1" 
          class="tab-button" 
          :class="{ active: activeTab === 'self' }" 
          @click="activeTab = 'self'"
        >
          <i class="fas fa-user-cog"></i> Aktionen
        </button>
        <button 
          v-if="adminLevel >= 1" 
          class="tab-button" 
          :class="{ active: activeTab === 'support' }" 
          @click="activeTab = 'support'"
        >
          <i class="fas fa-life-ring"></i> Support
        </button>
        <button 
          v-if="adminLevel >= 4" 
          class="tab-button" 
          :class="{ active: activeTab === 'vehicles' }" 
          @click="activeTab = 'vehicles'"
        >
          <i class="fas fa-car"></i> Fahrzeuge
        </button>
        <button 
          v-if="adminLevel >= 4" 
          class="tab-button" 
          :class="{ active: activeTab === 'houses' }" 
          @click="activeTab = 'houses'"
        >
          <i class="fas fa-home"></i> Häuser
        </button>
        <button 
          v-if="adminLevel >= 1" 
          class="tab-button" 
          :class="{ active: activeTab === 'communication' }" 
          @click="activeTab = 'communication'"
        >
          <i class="fas fa-bullhorn"></i> Kommunikation
        </button>
      </div>

      <!-- Content Wrapper -->
      <div class="content-wrapper">
        <!-- Tab: Spieler -->
        <div v-if="activeTab === 'players'" class="tab-content">
          <div class="grid-container">
            <div class="list-panel">
              <div 
                v-for="p in players" 
                :key="getPlayerId(p)" 
                class="list-item" 
                :class="{ selected: selectedPlayerId === getPlayerId(p) }"
                @click="selectPlayer(p)"
              >
                {{ p.name }} [ID: {{ getPlayerId(p) }}]
              </div>
            </div>

            <div class="action-panel">
              <div v-if="selectedPlayer">
                <h3>Aktionen für {{ selectedPlayer.name }}</h3>
                <div class="action-grid">
                  <button v-if="adminLevel >= 1" class="btn blue" @click="handleAction('goto')">GoTo</button>
                  <button v-if="adminLevel >= 2 && !isSelfTarget" class="btn blue" @click="handleAction('gethere')">GetHere</button>
                  <button v-if="adminLevel >= 1" class="btn green" @click="handleAction('heal')">Heilen</button>
                  <button v-if="adminLevel >= 1" class="btn green" @click="handleAction('revive')">Wiederbeleben</button>
                  <button v-if="adminLevel >= 1 && !isSelfTarget" class="btn yellow" @click="handleAction('toggleFreeze')">Freeze</button>
                  <button v-if="adminLevel >= 2 && !isSelfTarget" class="btn blue" @click="handleAction('toggleSpectate')">Spectate</button>
                  <button v-if="adminLevel >= 2 && !isSelfTarget" class="btn yellow" @click="handleAction('kick')">Kicken</button>
                  <button v-if="adminLevel >= 2 && !isSelfTarget" class="btn red" @click="handleAction('ban')">Bannen</button>
                  <button v-if="adminLevel >= 4 && !isSelfTarget" class="btn red" @click="handleAction('unban')">Entbannen</button>
                </div>

                <div v-if="isSelfTarget" style="margin-top: 20px;">
                  <h4>Admin-Status</h4>
                  <div class="action-grid">
                    <button v-if="adminLevel >= 1" class="btn green" @click="handleAction('toggleAduty')">Aduty An/Aus</button>
                    <button v-if="adminLevel >= 1" class="btn blue" @click="handleAction('toggleInvisibility')">Unsichtbar An/Aus</button>
                  </div>
                </div>

                <div v-if="adminLevel >= 4" style="margin-top: 20px;">
                  <h4>Attribute & Geschenke</h4>
                  <div class="input-group">
                    <input v-model.number="form.dimension" type="number" placeholder="Dimension">
                    <button class="btn blue" @click="handleAction('setPlayerDimension')">Dim setzen</button>
                  </div>
                  
                  <div class="input-group" style="margin-top: 10px;">
                    <input v-model.number="form.moneyAmount" type="number" placeholder="Betrag">
                    <select v-model="form.moneyType" class="btn" style="width: auto;">
                      <option value="cash">Bargeld</option>
                      <option value="bank">Bank</option>
                    </select>
                  </div>
                  <button class="btn blue" style="margin-top: 5px;" @click="handleAction('giveMoney')">Geld geben</button>

                  <input v-model="form.weaponName" type="text" placeholder="Waffen-Name (z.B. weapon_pistol)" style="margin-top: 10px;">
                  <button class="btn blue" @click="handleAction('giveWeapon')">Waffe geben</button>
                </div>
              </div>
              <div v-else class="empty-state">
                <p>Bitte wähle einen Spieler aus der Liste aus.</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Tab: Aktionen (Self) -->
        <div v-if="activeTab === 'self'" class="tab-content">
          <div class="action-group">
            <h3>Teleport</h3>
            <select v-model="form.selectedTpLocation" class="btn">
              <option disabled value="">Ort auswählen...</option>
              <option v-for="loc in tpLocations" :key="loc" :value="loc">{{ loc }}</option>
            </select>
            <div class="action-grid" style="margin-top:10px;">
              <button v-if="adminLevel >= 1" class="btn blue" @click="handleAction('teleportToLocation', false)">Teleportieren</button>
              <button v-if="adminLevel >= 2" class="btn blue" @click="handleAction('teleportToLocation', true)">Mit Fahrzeug</button>
            </div>
          </div>

          <div class="action-group" style="margin-top: 20px;">
            <h3>Allgemein</h3>
            <div class="action-grid">
              <button v-if="adminLevel >= 1" class="btn blue" @click="handleAction('spawnAdminVehicle')">Admin Fahrzeug</button>
              <button v-if="adminLevel >= 1" class="btn blue" @click="handleAction('goBack')">Zurück porten</button>
              <button v-if="adminLevel >= 1" class="btn yellow" @click="handleAction('toggleGodMode')">Godmode</button>
              <button v-if="adminLevel >= 1" class="btn yellow" @click="handleAction('toggleNoClip')">NoClip</button>
            </div>
          </div>

          <div class="action-group" style="margin-top: 20px;">
            <h3>Zu Koordinaten teleportieren</h3>
            <div class="input-group">
              <input v-model.number="form.coordX" type="number" placeholder="X">
              <input v-model.number="form.coordY" type="number" placeholder="Y">
              <input v-model.number="form.coordZ" type="number" placeholder="Z">
            </div>
            <button v-if="adminLevel >= 1" class="btn blue" style="margin-top: 10px;" @click="handleAction('teleportToCoords')">Teleportieren</button>
          </div>
        </div>

        <!-- Tab: Support -->
        <div v-if="activeTab === 'support'" class="tab-content">
          <div class="grid-container">
            <div class="list-panel">
              <div 
                v-for="ticket in supportTickets" 
                :key="ticket.Id" 
                class="list-item" 
                :class="{ selected: selectedTicketId === ticket.Id }"
                @click="selectTicket(ticket)"
              >
                {{ ticket.IsPriority ? '❗️ ' : '' }}{{ ticket.ReporterName }}: {{ (ticket.Message || '').substring(0, 25) }}...
              </div>
            </div>

            <div class="action-panel">
              <div v-if="selectedTicket">
                <h3>Ticket von {{ selectedTicket.ReporterName }}</h3>
                <div class="action-group">
                  <h4>Nachricht des Spielers</h4>
                  <div class="message-display">{{ selectedTicket.Message }}</div>
                </div>
                <hr>
                <div class="action-group">
                  <h4>Ticket-Verwaltung</h4>
                  <div class="action-grid">
                    <button class="btn green" @click="handleSupportTicketAction('updateStatus', 'Geschlossen')">Als Erledigt markieren</button>
                    <button 
                      class="btn" 
                      :class="selectedTicket.IsPriority ? 'yellow' : 'blue'" 
                      @click="handleSupportTicketAction('togglePriority')"
                    >
                      {{ selectedTicket.IsPriority ? 'Priorität Entfernen' : 'Priorität Setzen' }}
                    </button>
                  </div>
                </div>
                <hr>
                <div class="action-group">
                  <h4>Spieler-Aktionen</h4>
                  <div class="action-grid">
                    <button v-if="adminLevel >= 1" class="btn blue" @click="handleAction('gotoTicketPlayer')">GoTo Spieler</button>
                    <button v-if="adminLevel >= 1" class="btn green" @click="handleAction('healTicketPlayer')">Spieler Heilen</button>
                    <button v-if="adminLevel >= 1" class="btn green" @click="handleAction('reviveTicketPlayer')">Spieler Wiederbeleben</button>
                  </div>
                </div>
                <hr>
                <div class="action-group">
                  <h4>Bisherige Kommentare</h4>
                  <div class="message-display">
                    <template v-if="ticketComments.length > 0">
                      <p v-for="(c, idx) in ticketComments" :key="idx" style="margin-bottom: 5px; border-bottom: 1px solid #444; padding-bottom: 5px;">
                        <strong>[{{ c.Timestamp }} - {{ c.AdminName }}]:</strong><br>{{ c.Text }}
                      </p>
                    </template>
                    <template v-else>Kein Kommentar vorhanden.</template>
                  </div>
                </div>
                <hr>
                <div class="action-group">
                  <h4>Neuen Kommentar hinzufügen</h4>
                  <textarea v-model="form.newComment" placeholder="Füge eine neue Notiz hinzu..."></textarea>
                  <button class="btn blue" style="margin-top:10px;" @click="handleSupportTicketAction('addComment')">Kommentar speichern</button>
                </div>
              </div>
              <div v-else class="empty-state">
                <p>Bitte wähle ein Support-Ticket aus der Liste aus.</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Tab: Fahrzeuge -->
        <div v-if="activeTab === 'vehicles'" class="tab-content">
          <div class="action-group">
            <h3>Fahrzeug Erschaffen & Verwalten</h3>
            
            <div class="action-group">
              <h4>Temporäres Fahrzeug</h4>
              <input v-model="form.tempVehModel" type="text" placeholder="Modell">
              <button v-if="adminLevel >= 3" class="btn blue" @click="handleAction('spawnTempVehicle')">Spawnen</button>
            </div>
            <hr>
            
            <div class="action-group">
              <h4>Persistentes Fahrzeug</h4>
              <input v-model="form.persVehModel" type="text" placeholder="Modell">
              <input v-model.number="form.persVehOwner" type="number" placeholder="Besitzer Account-ID">
              <input v-model="form.persVehPlate" type="text" placeholder="Kennzeichen">
              <div class="input-group">
                <input v-model.number="form.persVehColor1" type="number" placeholder="Farbe 1">
                <input v-model.number="form.persVehColor2" type="number" placeholder="Farbe 2">
              </div>
              <button v-if="adminLevel >= 4" class="btn green" style="margin-top:5px;" @click="handleAction('createPersVehicle')">Erstellen</button>
            </div>
            <hr>

            <div class="action-group">
              <h4>Fraktionsfahrzeug</h4>
              <input v-model="form.factionVehModel" type="text" placeholder="Modell">
              <input v-model.number="form.factionId" type="number" placeholder="Fraktions-ID">
              <input v-model="form.factionPlate" type="text" placeholder="Nummernschild">
              <div class="input-group">
                <input v-model.number="form.factionVehColor1" type="number" placeholder="Farbe 1">
                <input v-model.number="form.factionVehColor2" type="number" placeholder="Farbe 2">
              </div>
              <button v-if="adminLevel >= 4" class="btn green" style="margin-top:5px;" @click="handleAction('createFactionVehicle')">Erstellen</button>
            </div>
            <hr>

            <div class="action-group">
              <h4>Aktionen nach DB-ID</h4>
              <input v-model.number="form.vehDbId" type="number" placeholder="Fahrzeug DB-ID">
              <div class="action-grid" style="margin-top:5px;">
                <button v-if="adminLevel >= 4" class="btn blue" @click="handleAction('tptoVehicle')">TP zu Fzg.</button>
                <button v-if="adminLevel >= 4" class="btn yellow" @click="handleAction('parkVehicleInAlta')">In Alta Garage parken</button>
                <button v-if="adminLevel >= 4" class="btn green" @click="handleAction('fetchVehicle')">Fzg. herholen</button>
                <button v-if="adminLevel >= 4" class="btn blue" @click="handleAction('repairVehicle')">Fzg. reparieren</button>
                <button v-if="adminLevel >= 5" class="btn red" @click="confirmAndDeleteVehicle">Fzg. (DB) löschen</button>
              </div>
            </div>

            <div class="action-group">
              <h4>Nahes Fahrzeug</h4>
              <div class="action-grid">
                <button v-if="adminLevel >= 4" class="btn yellow" @click="handleAction('forceToggleLock', true)">Abschließen</button>
                <button v-if="adminLevel >= 4" class="btn green" @click="handleAction('forceToggleLock', false)">Aufschließen</button>
                <button v-if="adminLevel >= 4" class="btn green" @click="handleAction('forceToggleEngine', true)">Motor an</button>
                <button v-if="adminLevel >= 4" class="btn red" @click="handleAction('forceToggleEngine', false)">Motor aus</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Tab: Häuser -->
        <div v-if="activeTab === 'houses'" class="tab-content">
          <div class="grid-container">
            <div class="list-panel">
              <div 
                v-for="house in houses" 
                :key="getHouseId(house)" 
                class="list-item" 
                :class="{ selected: selectedHouseId === getHouseId(house) }"
                @click="selectHouse(house)"
              >
                {{ house.name }} [{{ getHouseId(house) }}] ({{ house.owner }})
              </div>
            </div>

            <div class="action-panel">
              <div v-if="selectedHouse">
                <h3>Aktionen für {{ selectedHouse.name }}</h3>
                <div class="action-group">
                  <h4>Aktionen</h4>
                  <div class="action-grid">
                    <button v-if="adminLevel >= 4" class="btn blue" @click="handleAction('teleportToHouse')">Teleportieren</button>
                  </div>
                </div>
                <hr>
                <div class="action-group">
                  <h4>Besitzer ändern</h4>
                  <input v-model.number="form.houseOwnerId" type="number" placeholder="Neue Besitzer Account-ID (0 = Staat)">
                  <button v-if="adminLevel >= 4" class="btn green" style="margin-top:10px;" @click="handleAction('setHouseOwner')">Besitzer setzen</button>
                </div>
              </div>
              <div v-else class="empty-state">
                <p>Bitte wähle ein Haus aus der Liste aus.</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Tab: Kommunikation -->
        <div v-if="activeTab === 'communication'" class="tab-content">
          <div class="action-group">
            <h3>Admin-Chat</h3>
            <textarea v-model="form.adminChatMessage" placeholder="Nachricht an alle Admins..." rows="5"></textarea>
            <button v-if="adminLevel >= 1" class="btn blue" style="margin-top:5px;" @click="handleAction('sendAdminChat')">Senden</button>
          </div>
          <div class="action-group" style="margin-top: 20px;">
            <h3>Server-Ankündigung</h3>
            <textarea v-model="form.announcementMessage" placeholder="Ankündigung an alle Spieler..." rows="5"></textarea>
            <button v-if="adminLevel >= 2" class="btn yellow" style="margin-top:5px;" @click="handleAction('sendAnnouncement')">Ankündigung senden</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted, onUnmounted } from 'vue';
import { sendNuiCallback } from '../services/nui';

// --- TypeScript Interfaces ---
interface Player {
  id?: number;
  Id?: number;
  name: string;
}

interface House {
  id?: number;
  Id?: number;
  name: string;
  owner: string;
}

interface TicketComment {
  Timestamp: string;
  AdminName: string;
  Text: string;
}

interface SupportTicket {
  Id: number;
  ReporterId: number;
  ReporterName: string;
  Message: string;
  IsPriority: boolean;
  AdminComment?: string | TicketComment[];
}

interface NuiPayload {
  adminLevel?: number;
  ownAccountId?: number;
  players?: Player[] | string;
  houses?: House[] | string;
  tpLocations?: string[] | string;
  tickets?: SupportTicket[] | string;
}

interface NuiMessageEventData {
  action: string;
  payload: NuiPayload;
}

// --- Dynamic State ---
const isVisible = ref<boolean>(true); // Zum Testen direkt auf true gesetzt
const activeTab = ref<string>('players');

const adminLevel = ref<number>(4); // Für die Entwicklungsansicht hochgesetzt
const ownAccountId = ref<number>(1);
const players = ref<Player[]>([
  { id: 1, name: 'Max_Mustermann' },
  { id: 2, name: 'Erika_Musterfrau' }
]);
const houses = ref<House[]>([
  { id: 101, name: 'Villa Vinewood', owner: 'Max_Mustermann' },
  { id: 102, name: 'Appartement Mirror Park', owner: 'Staat' }
]);
const tpLocations = ref<string[]>(['Legion Square', 'Sandy Shores', 'Paleto Bay']);
const supportTickets = ref<SupportTicket[]>([]);

// Selection State
const selectedPlayerId = ref<number | null>(null);
const selectedHouseId = ref<number | null>(null);
const selectedTicketId = ref<number | null>(null);

// Form Reactive Object
const form = reactive({
  dimension: null as number | null,
  moneyAmount: null as number | null,
  moneyType: 'cash' as 'cash' | 'bank',
  weaponName: '',
  selectedTpLocation: '',
  coordX: null as number | null,
  coordY: null as number | null,
  coordZ: null as number | null,
  newComment: '',
  tempVehModel: '',
  persVehModel: '',
  persVehOwner: null as number | null,
  persVehPlate: '',
  persVehColor1: 0,
  persVehColor2: 0,
  factionVehModel: '',
  factionId: null as number | null,
  factionPlate: '',
  factionVehColor1: 0,
  factionVehColor2: 0,
  vehDbId: null as number | null,
  houseOwnerId: null as number | null,
  adminChatMessage: '',
  announcementMessage: ''
});

// Helper zur Auswertung von IDs
const getPlayerId = (p: Player): number => p.id ?? p.Id ?? 0;
const getHouseId = (h: House): number => h.id ?? h.Id ?? 0;

// Dynamic Computed Helpers
const selectedPlayer = computed(() => players.value.find(p => getPlayerId(p) === selectedPlayerId.value));
const selectedHouse = computed(() => houses.value.find(h => getHouseId(h) === selectedHouseId.value));
const selectedTicket = computed(() => supportTickets.value.find(t => t.Id === selectedTicketId.value));
const isSelfTarget = computed(() => selectedPlayerId.value === ownAccountId.value);

const ticketComments = computed<TicketComment[]>(() => {
  if (!selectedTicket.value || !selectedTicket.value.AdminComment) return [];
  const comment = selectedTicket.value.AdminComment;
  if (typeof comment === 'string' && comment.startsWith('[')) {
    try { return JSON.parse(comment); } catch { return []; }
  }
  if (Array.isArray(comment)) return comment;
  return [{ Timestamp: 'Früher', AdminName: 'System', Text: comment as string }];
});

const closePanel = () => {
  isVisible.value = false;
  sendNuiCallback('admin:closePanel');
};

const selectPlayer = (p: Player) => {
  selectedPlayerId.value = getPlayerId(p);
};

const selectHouse = (h: House) => {
  selectedHouseId.value = getHouseId(h);
};

const selectTicket = (t: SupportTicket) => {
  selectedTicketId.value = t.Id;
  selectedPlayerId.value = t.ReporterId;
};

const handleSupportTicketAction = (action: string, param?: any) => {
  if (!selectedTicketId.value) return alert('Wähle zuerst ein Ticket aus.');
  const ticket = selectedTicket.value;
  if (!ticket) return;

  if (action === 'updateStatus') {
    sendNuiCallback('admin:support:updateStatus', { ticketId: selectedTicketId.value, status: param });
    if (param === 'Geschlossen') closePanel();
  } else if (action === 'togglePriority') {
    const newPrio = !ticket.IsPriority;
    ticket.IsPriority = newPrio;
    sendNuiCallback('admin:support:setPriority', { ticketId: selectedTicketId.value, isPriority: newPrio });
  } else if (action === 'addComment') {
    if (!form.newComment.trim()) return;
    sendNuiCallback('admin:support:addComment', { ticketId: selectedTicketId.value, comment: form.newComment });
    if (!Array.isArray(ticket.AdminComment)) {
      ticket.AdminComment = ticketComments.value;
    }
    (ticket.AdminComment as TicketComment[]).push({ Timestamp: 'Jetzt', AdminName: 'Ich', Text: form.newComment });
    form.newComment = '';
  }
};

const confirmAndDeleteVehicle = () => {
  if (form.vehDbId && confirm(`Willst du das Fahrzeug mit der ID ${form.vehDbId} wirklich zerstören? Es wird damit unwiderruflich gelöscht.`)) {
    sendNuiCallback('admin:deleteVehicleDB', { vehId: form.vehDbId });
  }
};

const handleAction = (action: string, param?: any) => {
  // Spieler-bezogene Aktionen
  if (['goto', 'gethere', 'heal', 'revive', 'kick', 'ban', 'unban', 'giveMoney', 'giveWeapon', 'toggleSpectate', 'toggleFreeze', 'setPlayerDimension'].includes(action)) {
    if (!selectedPlayerId.value) return alert('Wähle zuerst einen Spieler aus.');
    
    if (action === 'kick' || action === 'ban') {
      const reason = prompt(`Grund für ${action}:`, 'Regelverstoß');
      if (reason) sendNuiCallback(`admin:${action}`, { targetId: selectedPlayerId.value, reason });
    } else if (action === 'giveMoney') {
      if (form.moneyAmount && form.moneyAmount > 0) {
        sendNuiCallback('admin:giveMoney', { targetId: selectedPlayerId.value, amount: form.moneyAmount, type: form.moneyType });
      }
    } else if (action === 'giveWeapon') {
      if (form.weaponName) sendNuiCallback('admin:giveWeapon', { targetId: selectedPlayerId.value, weapon: form.weaponName });
    } else if (action === 'setPlayerDimension') {
      if (form.dimension !== null) sendNuiCallback('admin:setPlayerDimension', { targetId: selectedPlayerId.value, dim: form.dimension });
    } else {
      sendNuiCallback(`admin:${action}`, { targetId: selectedPlayerId.value });
      if (action === 'goto') closePanel();
    }
  } 
  // Ticket-Spieler Schnellaktionen
  else if (['gotoTicketPlayer', 'healTicketPlayer', 'reviveTicketPlayer'].includes(action)) {
    if (!selectedTicket.value) return;
    const pId = selectedTicket.value.ReporterId;
    const realAction = action.replace('TicketPlayer', '');
    sendNuiCallback(`admin:${realAction}`, { targetId: pId });
    if (realAction === 'goto') closePanel();
  }
  // Häuser-Aktionen
  else if (['teleportToHouse', 'setHouseOwner'].includes(action)) {
    if (!selectedHouseId.value) return alert('Wähle zuerst ein Haus aus.');
    if (action === 'setHouseOwner') {
      if (form.houseOwnerId !== null) sendNuiCallback('admin:setHouseOwner', { houseId: selectedHouseId.value, ownerId: form.houseOwnerId });
    } else {
      sendNuiCallback('admin:teleportToHouse', { houseId: selectedHouseId.value });
      closePanel();
    }
  } 
  // Fahrzeug-Aktionen
  else if (['spawnTempVehicle', 'createPersVehicle', 'createFactionVehicle', 'tptoVehicle', 'parkVehicleInAlta', 'forceToggleLock', 'forceToggleEngine', 'fetchVehicle', 'repairVehicle'].includes(action)) {
    if (['tptoVehicle', 'parkVehicleInAlta', 'fetchVehicle', 'repairVehicle'].includes(action)) {
      if (form.vehDbId !== null) sendNuiCallback(`admin:${action}`, { vehId: form.vehDbId });
    } else if (action === 'spawnTempVehicle') {
      if (form.tempVehModel) sendNuiCallback('admin:spawnTempVehicle', { model: form.tempVehModel });
    } else if (action === 'createPersVehicle') {
      if (form.persVehModel && form.persVehOwner !== null) {
        sendNuiCallback('admin:createPersVehicle', {
          model: form.persVehModel,
          ownerId: form.persVehOwner,
          color1: form.persVehColor1 || 0,
          color2: form.persVehColor2 || 0,
          plate: form.persVehPlate || 'NEU'
        });
      }
    } else if (action === 'createFactionVehicle') {
      if (form.factionVehModel && form.factionId !== null && form.factionPlate) {
        sendNuiCallback('admin:createFactionVehicle', {
          model: form.factionVehModel,
          factionId: form.factionId,
          plate: form.factionPlate,
          color1: form.factionVehColor1 || 0,
          color2: form.factionVehColor2 || 0
        });
      }
    } else if (action === 'forceToggleLock' || action === 'forceToggleEngine') {
      sendNuiCallback(`admin:${action}`, { status: param });
    }
  } 
  // Teleports & Positionen
  else if (action === 'teleportToCoords') {
    if (form.coordX !== null && form.coordY !== null && form.coordZ !== null) {
      sendNuiCallback('admin:teleportToCoords', { x: form.coordX, y: form.coordY, z: form.coordZ });
      closePanel();
    }
  } else if (action === 'teleportToLocation') {
    if (form.selectedTpLocation) {
      sendNuiCallback('admin:teleportToLocation', { location: form.selectedTpLocation, withVeh: param });
      closePanel();
    }
  } 
  // Selbst- & Chat-Aktionen
  else if (['sendAdminChat', 'sendAnnouncement', 'spawnAdminVehicle', 'goBack', 'toggleGodMode', 'toggleNoClip', 'toggleInvisibility', 'toggleAduty'].includes(action)) {
    if (action === 'sendAdminChat') {
      if (form.adminChatMessage) {
        sendNuiCallback('admin:sendAdminChat', { message: form.adminChatMessage });
        form.adminChatMessage = '';
      }
    } else if (action === 'sendAnnouncement') {
      if (form.announcementMessage) {
        sendNuiCallback('admin:sendAnnouncement', { message: form.announcementMessage });
        form.announcementMessage = '';
        closePanel();
      }
    } else {
      sendNuiCallback(`admin:${action}`);
      if (['toggleNoClip', 'toggleInvisibility', 'toggleAduty', 'spawnAdminVehicle'].includes(action)) {
        closePanel();
      }
    }
  }
};

// Listen für Ingame Focus / Events via JavaScript Event Loop
const handleNuiMessage = (event: MessageEvent<NuiMessageEventData>) => {
  const { action, payload } = event.data;

  if (action === 'openAdminPanel' && payload) {
    adminLevel.value = payload.adminLevel ?? 4;
    ownAccountId.value = payload.ownAccountId || 0;
    players.value = typeof payload.players === 'string' ? JSON.parse(payload.players) : (payload.players || []);
    houses.value = typeof payload.houses === 'string' ? JSON.parse(payload.houses) : (payload.houses || []);
    tpLocations.value = typeof payload.tpLocations === 'string' ? JSON.parse(payload.tpLocations) : (payload.tpLocations || []);
    supportTickets.value = typeof payload.tickets === 'string' ? JSON.parse(payload.tickets) : (payload.tickets || []);
    
    if (players.value.length > 0) {
      selectedPlayerId.value = getPlayerId(players.value[0]);
    }
    isVisible.value = true;
  } else if (action === 'closeAdminPanel') {
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
.container {
  --bg-primary: rgba(30, 30, 30, 0.98);
  --bg-secondary: rgba(20, 20, 20, 0.98);
  --bg-tertiary: rgba(45, 45, 45, 0.95);
  --text-primary: #f0f0f0;
  --text-secondary: #a0a0a0;
  --border-color: rgba(0, 255, 255, 0.5);
  --accent-cyan: #00ffff;
  --accent-cyan-glow: rgba(0, 255, 255, 0.4);
  --accent-red: #dc3545;
  --accent-yellow: #ffc107;
  --accent-green: #28a745;

  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background-color: var(--bg-primary);
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 0 25px var(--accent-cyan-glow);
  width: 950px;
  height: 700px;
  border: 1px solid var(--border-color);
  backdrop-filter: blur(8px);
  display: flex;
  flex-direction: column;
  font-family: 'Roboto', 'Arial', sans-serif;
  color: var(--text-primary);
  user-select: none;
  font-size: 14px;
  z-index: 99999;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 15px;
}

.header h2 {
  color: var(--accent-cyan);
  margin: 0;
  text-shadow: 0 0 8px var(--accent-cyan-glow);
  font-size: 24px;
  font-weight: 700;
}

.close-button {
  background: transparent;
  border: none;
  color: var(--text-secondary);
  font-size: 24px;
  cursor: pointer;
}

.close-button:hover {
  color: #fff;
}

.main-body {
  display: flex;
  flex-grow: 1;
  height: calc(100% - 78px);
  overflow: hidden;
}

.tabs {
  width: 180px;
  background: var(--bg-secondary);
  padding: 10px 0;
  border-right: 1px solid #444;
  flex-shrink: 0;
  overflow-y: auto;
}

.tab-button {
  background-color: transparent;
  color: var(--text-secondary);
  border: none;
  padding: 12px 18px;
  cursor: pointer;
  font-size: 15px;
  transition: all 0.2s ease;
  width: 100%;
  text-align: left;
  border-right: 3px solid transparent;
}

.tab-button:hover {
  color: #fff;
  background-color: rgba(255, 255, 255, 0.05);
}

.tab-button.active {
  color: var(--accent-cyan);
  background-color: rgba(0, 255, 255, 0.1);
  border-right: 3px solid var(--accent-cyan);
  font-weight: bold;
}

.content-wrapper {
  flex-grow: 1;
  padding: 20px;
  overflow-y: auto;
  min-height: 0;
  background: rgba(15, 15, 15, 0.5);
}

.tab-content {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.grid-container {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 20px;
  height: 100%;
  min-height: 0;
}

.list-panel {
  background: rgba(0, 0, 0, 0.4);
  border: 1px solid #444;
  border-radius: 5px;
  overflow-y: auto;
  height: 100%;
  max-height: 520px;
}

.list-item {
  padding: 10px 15px;
  border-bottom: 1px solid #333;
  cursor: pointer;
  color: var(--text-primary);
}

.list-item:hover {
  background-color: rgba(255, 255, 255, 0.1);
}

.list-item.selected {
  background: var(--accent-cyan);
  color: #111;
  font-weight: bold;
}

.action-panel {
  background: rgba(0, 0, 0, 0.2);
  border: 1px solid #333;
  padding: 15px;
  border-radius: 5px;
  overflow-y: auto;
  max-height: 520px;
}

.action-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
  gap: 10px;
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: var(--text-secondary);
  font-style: italic;
}

.btn {
  width: 100%;
  padding: 10px;
  margin-top: 5px;
  border: none;
  color: #fff;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
  transition: all 0.2s ease;
}

.btn:hover {
  filter: brightness(1.2);
}

.btn.blue { background-color: #007bff; }
.btn.red { background-color: var(--accent-red); }
.btn.yellow { background-color: var(--accent-yellow); color: #111; }
.btn.green { background-color: var(--accent-green); }

.input-group {
  display: flex;
  gap: 10px;
}

input, textarea, select {
  width: 100%;
  padding: 10px;
  margin-top: 5px;
  background: #222;
  border: 1px solid #444;
  color: var(--text-primary);
  border-radius: 4px;
}

.message-display {
  background: rgba(0, 0, 0, 0.5);
  padding: 10px;
  border-radius: 4px;
  border: 1px solid #444;
}
</style>