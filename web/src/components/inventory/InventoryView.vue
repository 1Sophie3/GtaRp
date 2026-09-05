<template>
  <div v-if="isVisible" class="inventory-wrapper">
    <InventoryList title="Inventar" @sort="sortItems">
      <Container group-name="inventory" :get-child-payload="getChildPayload" @drop="onDrop">
        <Draggable v-for="item in inventoryItems" :key="item.id">
          <InventoryItem 
            :item="item" 
            @use="handleItemAction('use', $event)"
            @drop="handleItemAction('drop', $event)"
            @give="handleItemAction('give', $event)"
          />
        </Draggable>
      </Container>
    </InventoryList>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { Container, Draggable } from 'vue3-smooth-dnd';
import InventoryList from './InventoryList.vue';
import InventoryItem from './InventoryItem.vue';

const isVisible = ref(false);
const inventoryItems = ref([]);

const getChildPayload = (index) => inventoryItems.value[index];

const onDrop = (dropResult) => {
  const { removedIndex, addedIndex, payload } = dropResult;
  if (removedIndex === null && addedIndex === null) return;

  const result = [...inventoryItems.value];
  let itemToAdd = payload;

  if (removedIndex !== null) {
    itemToAdd = result.splice(removedIndex, 1)[0];
  }
  if (addedIndex !== null) {
    result.splice(addedIndex, 0, itemToAdd);
  }

  inventoryItems.value = result;
  
  // NUI-Callback an C# senden, wenn Items umsortiert wurden
  fetch(`https://${GetParentResourceName()}/saveInventoryOrder`, {
    method: 'POST',
    body: JSON.stringify(inventoryItems.value)
  });
};

const sortItems = () => {
  // Items nach Name zusammenlegen / summieren
  const itemMap = new Map();
  inventoryItems.value.forEach(item => {
    if (itemMap.has(item.name)) {
      itemMap.get(item.name).amount += item.amount;
    } else {
      itemMap.set(item.name, { ...item });
    }
  });
  inventoryItems.value = Array.from(itemMap.values());
};

const handleItemAction = (action, item) => {
  fetch(`https://${GetParentResourceName()}/triggerItemAction`, {
    method: 'POST',
    body: JSON.stringify({ action, itemId: item.id, itemData: item })
  });
};

const handleNuiMessage = (event) => {
  const { action, data } = event.data;

  if (action === 'openInventory') {
    inventoryItems.value = data.items;
    isVisible.value = true;
  } else if (action === 'closeInventory') {
    isVisible.value = false;
  }
};

onMounted(() => {
  window.addEventListener('message', handleNuiMessage);
  
  // ESC-Taste schließt das Inventar
  window.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && isVisible.value) {
      fetch(`https://${GetParentResourceName()}/closeInventory`, { method: 'POST' });
      isVisible.value = false;
    }
  });
});

onUnmounted(() => {
  window.removeEventListener('message', handleNuiMessage);
});
</script>

<style scoped>
.inventory-wrapper {
  position: absolute;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: rgba(0, 0, 0, 0.4);
}
</style>