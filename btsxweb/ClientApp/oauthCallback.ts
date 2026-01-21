import { createApp } from 'vue';
import OAuthCallbackApp from './components/OAuthCallbackApp.vue';

const successElement = document.getElementById('oauthSuccess') as HTMLInputElement;
const tokenElement = document.getElementById('oauthToken') as HTMLInputElement;
const emailElement = document.getElementById('oauthEmail') as HTMLInputElement;
const serverTypeElement = document.getElementById('oauthServerType') as HTMLInputElement;
const errorElement = document.getElementById('oauthError') as HTMLInputElement;

const success = successElement?.value === 'true';
const token = tokenElement?.value || '';
const email = emailElement?.value || '';
const serverType = serverTypeElement?.value || '';
const error = errorElement?.value || '';

const app = createApp(OAuthCallbackApp, {
    success: success,
    token: token,
    email: email,
    serverType: serverType,
    error: error
});

app.mount('#app');
