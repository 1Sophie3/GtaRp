<template>
  <div class="inventory-list">
    <div class="title">{{ title }}</div>
    <div class="items-container">
      <slot></slot>
    </div>
    <button type="button" class="btn-sort" @click="$emit('sort')">Items zusammenlegen</button>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, default: 'Inventar' }
});

defineEmits(['sort']);
</script>

<style scoped>
.inventory-list {
  background-color: rgba(30, 30, 30, 0.95);
  border: 1px solid rgba(0, 255, 255, 0.4);
  padding: 15px;
  margin: 0 10px;
  border-radius: 8px;
  width: 350px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.5);
}

.title {
  color: #ffffff;
  padding-bottom: 8px;
  font-size: 18px;
  font-weight: 600;
  text-align: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
  margin-bottom: 10px;
}

.items-container {
  max-height: 500px;
  overflow-y: auto;
  padding-right: 5px;
}

.btn-sort {
  margin-top: 15px;
  background-color: #00ffff;
  color: #000;
  border: none;
  padding: 8px 12px;
  font-weight: bold;
  border-radius: 4px;
  cursor: pointer;
  width: 100%;
  transition: background 0.2s;
}

.btn-sort:hover {
  background-color: #00cccc;
}
</style>