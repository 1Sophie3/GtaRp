<!-- 
<template> 
    <div class="inventory"> 
        <InventoryList title="Inventar" :arrayitem="inventoryItems" style="max-height: 715px;overflow-y: scroll">
            <Contrainer group-name="1" get-child-payload="getChildPayload" @drop="onDrop('inventoryItems', $event)">
                <Draggable v-for="item in inventoryItems" :key="item.id">
                    <InventoryItem :item="item" class="mt-2" title="Inventar" :arrayitems="inventoryItems">

                    </InventoryItem>

                </Draggable> 
            </Contrainer>
        </InventoryList>      
    </div>
</template>

<script>
import { Draggable, Contrainer } from 'vue-smooth-dnd'; // Import Draggable
import inventoryList from './utils/inventoryList.vue';
import inventoryItem from './utils/inventoryItem.vue';

export default {
    data() {
        return {
            InventoryItems: [{}],
        }
    },
    components: {
        Draggable,
        Contrainer,
        inventoryList,
        inventoryItem
  },
  mothods: {
    getChildPayload: function(index) {
        return this.inventoryItems[index];
    },  
  }
}
</script>
<style> 
.inventory {
    background-color: transparent !important;
    background: transparent !important;    
    display: flex;
    justify-content: center;
    margin: 235px 0;

}
::-webkit-scrollbar {
    width: 3px;
}
::-webkit-scrollbar-track {
    box-shadow: inset 0 0 5px grey;
    border-radius: 10px;
}
::-webkit-scrollbar-thumb
{
    background: #eee; 
    border-radius: 10px;
}
</style> -->