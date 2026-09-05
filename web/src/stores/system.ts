import { reactive } from 'vue';

export interface SystemState {
  isConnected: boolean;
  lastError: string | null;
  checkBackendConnection: () => Promise<void>;
}

export const systemState = reactive<SystemState>({
  isConnected: false,
  lastError: null,

  async checkBackendConnection(): Promise<void> {
    try {
      const response = await fetch('https://dein-resource-name/pingBackend', { 
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
      });
      
      if (response.ok) {
        this.isConnected = true;
        this.lastError = null;
      } else {
        throw new Error('Server antwortet nicht');
      }
    } catch (err) {
      this.isConnected = false;
      this.lastError = 'Verbindung derzeit nicht möglich. Keine Live-Daten vorhanden.';
    }
  }
});