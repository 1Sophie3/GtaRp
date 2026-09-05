<template>
  <div v-if="!systemState.isConnected" class="offline-warning">
    <span class="icon">⚠️</span>
    <span class="text">Verbindung derzeit nicht möglich. Es werden temporäre Daten (Mock) angezeigt.</span>
  </div>
</template>

<script setup lang="ts">
import { systemState } from '../../stores/system';
</script>

<style scoped>
.offline-warning {
  background-color: rgba(239, 68, 68, 0.2);
  border: 1px solid #ef4444;
  color: #fca5a5;
  padding: 8px 12px;
  border-radius: 6px;
  margin-bottom: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
}
</style>