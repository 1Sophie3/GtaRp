<template>
  <div v-if="isVisible" class="death-wrapper">
    <div class="container">
      <h1>Schwer verletzt</h1>
      <p>Bitte warte, bis Hilfe eintrifft.</p>
      <div class="countdown">{{ formattedTime }}</div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';

// Prüft, ob wir im Standalone-Browser/Dev-Modus testen
const isDevMode = import.meta.env.DEV && typeof window.GetParentResourceName !== 'function';

// Im Dev-Modus direkt sichtbar schalten, im Spiel standardmäßig unsichtbar
const isVisible = ref(isDevMode);
const secondsRemaining = ref(600);
let devInterval = null;

// Formatiert Sekunden in ein sauberes MM:SS Format (z.B. 10:00)
const formattedTime = computed(() => {
  const minutes = Math.floor(secondsRemaining.value / 60);
  const seconds = secondsRemaining.value % 60;
  return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
});

const handleNuiMessage = (event) => {
  const { action, seconds } = event.data;

  if (action === 'startDeathEffect') {
    isVisible.value = true;
    if (seconds !== undefined) secondsRemaining.value = seconds;
  } else if (action === 'stopDeathEffect') {
    isVisible.value = false;
  } else if (action === 'updateDeathTimer') {
    secondsRemaining.value = seconds;
  }
};

onMounted(() => {
  window.addEventListener('message', handleNuiMessage);

  // Zähler-Simulation NUR für lokales Browser-Debugging
  if (isDevMode) {
    devInterval = setInterval(() => {
      if (secondsRemaining.value > 0) {
        secondsRemaining.value--;
      }
    }, 1000);
  }
});

onUnmounted(() => {
  window.removeEventListener('message', handleNuiMessage);
  if (devInterval) clearInterval(devInterval);
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css?family=Cinzel:400,700&display=swap');

.death-wrapper {
  position: absolute;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  font-family: 'Cinzel', serif;
  color: #fff;
  /* Im Browser mit dunklem Schleier für Vorschau, im Spiel transparent für Ingame-Effekte */
  background: rgba(0, 0, 0, 0.6);
  pointer-events: none;
  display: flex;
  justify-content: center;
  align-items: center;
}

.container {
  text-align: center;
  padding: 20px 30px;
}

h1 {
  font-size: 48px;
  margin-bottom: 10px;
  color: #e74c3c;
  text-shadow: 2px 2px 8px rgba(0, 0, 0, 0.9);
}

p {
  font-size: 24px;
  margin-bottom: 20px;
  text-shadow: 1px 1px 4px rgba(0, 0, 0, 0.8);
}

.countdown {
  font-size: 36px;
  font-weight: bold;
  padding: 15px 30px;
  border: 3px solid #fff;
  border-radius: 50px;
  display: inline-block;
  text-shadow: 1px 1px 4px rgba(0, 0, 0, 0.8);
  box-shadow: 0 0 15px rgba(255, 255, 255, 0.2);
}
</style>