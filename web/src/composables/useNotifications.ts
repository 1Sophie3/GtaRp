import { ref } from 'vue';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface NotificationItem {
  id: number;
  text: string;
  type: NotificationType;
  duration?: number;
}

const notifications = ref<NotificationItem[]>([]);
let count = 0;

export function useNotifications() {
  const addNotification = (
    text: string, 
    type: NotificationType = 'info', 
    duration: number = 4000
  ) => {
    const id = ++count;
    notifications.value.push({ id, text, type, duration });

    if (duration > 0) {
      setTimeout(() => {
        removeNotification(id);
      }, duration);
    }
  };

  const removeNotification = (id: number) => {
    notifications.value = notifications.value.filter(item => item.id !== id);
  };

  return {
    notifications,
    addNotification,
    removeNotification
  };
}