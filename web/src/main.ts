import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router' // 1. Router importieren
import { initMockEnvironment } from './services/mock'
import '@fortawesome/fontawesome-free/css/all.min.css';

const app = createApp(App)

app.use(createPinia())
app.use(router) // 2. Router bei der Vue-App registrieren!

// Mocks vor der App initialisieren
initMockEnvironment()

app.mount('#app')