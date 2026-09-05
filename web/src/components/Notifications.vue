<template>
  <div class="toast-container">
    <TransitionGroup name="toast">
      <div 
        v-for="item in notifications" 
        :key="item.id" 
        :class="['toast-item', item.type]"
        @click="removeNotification(item.id)"
      >
        <div class="toast-icon">
          <span v-if="item.type === 'success'">✓</span>
          <span v-else-if="item.type === 'error'">✕</span>
          <span v-else-if="item.type === 'warning'">⚠</span>
          <span v-else>ℹ</span>
        </div>
        <div class="toast-content">
          <p class="toast-text">{{ item.text }}</p>
        </div>
        <button class="toast-close">&times;</button>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { useNotifications, type NotificationType } from '../composables/useNotifications';

const { notifications, addNotification, removeNotification } = useNotifications();

const handleNuiMessage = (event: MessageEvent) => {
  const { action, text, type, duration } = event.data || {};

  if (action === 'showNotification' && text) {
    const validTypes: NotificationType[] = ['success', 'error', 'warning', 'info'];
    const toastType: NotificationType = validTypes.includes(type) ? type : 'info';
    addNotification(text, toastType, duration || 4000);
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
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 999999;
  display: flex;
  flex-direction: column;
  gap: 10px;
  pointer-events: none;
  max-width: 350px;
  width: 100%;
}

.toast-item {
  pointer-events: auto;
  display: flex;
  align-items: center;
  padding: 12px 16px;
  border-radius: 6px;
  background-color: rgba(20, 20, 20, 0.95);
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
  font-family: 'Arial', sans-serif;
  cursor: pointer;
  overflow: hidden;
  border-left: 5px solid #00ffff;
  transition: all 0.3s ease;
}

/* Typen & Farben */
.toast-item.success {
  border-left-color: #2ece72;
}
.toast-item.success .toast-icon {
  color: #2ece72;
}

.toast-item.error {
  border-left-color: #e74c3c;
}
.toast-item.error .toast-icon {
  color: #e74c3c;
}

.toast-item.warning {
  border-left-color: #f1c40f;
}
.toast-item.warning .toast-icon {
  color: #f1c40f;
}

.toast-item.info {
  border-left-color: #3498db;
}
.toast-item.info .toast-icon {
  color: #3498db;
}

.toast-icon {
  font-size: 18px;
  font-weight: bold;
  margin-right: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.toast-content {
  flex-grow: 1;
}

.toast-text {
  margin: 0;
  font-size: 14px;
  line-height: 1.4;
  word-break: break-word;
}

.toast-close {
  background: transparent;
  border: none;
  color: #888888;
  font-size: 18px;
  margin-left: 10px;
  cursor: pointer;
  line-height: 1;
}

.toast-close:hover {
  color: #ffffff;
}

/* Vue Animations */
.toast-enter-from {
  opacity: 0;
  transform: translateX(50px);
}
.toast-enter-to {
  opacity: 1;
  transform: translateX(0);
}
.toast-leave-from {
  opacity: 1;
  transform: translateX(0);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(50px);
}
</style>