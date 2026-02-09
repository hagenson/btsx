import { createApp } from 'vue';
import * as signalR from '@microsoft/signalr';
import ContactsApp from './components/ContactsApp.vue';

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/contactHub")
    .withAutomaticReconnect()
    .build();

connection.start().catch((err) => {
    console.error(err.toString());
});

const app = createApp(ContactsApp, {
    connection: connection
});

app.mount('#app');
