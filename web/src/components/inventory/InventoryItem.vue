<template>
  <div class="inventory-item" v-if="item">
    <div class="item-info">
      <img :src="itemIcon" class="item-icon" alt="item" />
      <span class="item-name">{{ item.label }} x{{ item.amount }}</span>
    </div>

    <div class="item-actions">
      <!-- Verwenden / Nutzen -->
      <i class="fas fa-play action-icon" title="Benutzen" @click="$emit('use', item)"></i>
      <!-- Wegwerfen -->
      <i class="fas fa-trash-alt action-icon" title="Wegwerfen" @click="$emit('drop', item)"></i>
      <!-- Übergeben / Geben -->
      <i class="fas fa-hand-paper action-icon" title="Geben" @click="$emit('give', item)"></i>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  item: { type: Object, required: true }
});

defineEmits(['use', 'drop', 'give']);

// Dynamisches Bild-Laden für Vue 3 (Vite)
const itemIcon = computed(() => {
  return new URL(`../../assets/images/inventory/${props.item.name}.png`, import.meta.url).href;
});
</script>

<style scoped>
.inventory-item {
  background-color: #5c5b5b;
  padding: 8px 12px;
  margin-top: 8px;
  border-radius: 4px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.item-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.item-icon {
  width: 24px;
  height: 24px;
  object-fit: contain;
}

.item-name {
  color: #ffffff;
  font-size: 14px;
  font-weight: 500;
}

.item-actions {
  display: flex;
  gap: 10px;
}

.action-icon {
  color: #ccc;
  font-size: 14px;
  cursor: pointer;
  transition: color 0.2s;
}

.action-icon:hover {
  color: #00ffff;
}
</style>