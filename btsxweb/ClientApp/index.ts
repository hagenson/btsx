import { createApp } from 'vue';
import * as signalR from '@microsoft/signalr';
import IndexApp from './components/IndexApp.vue';

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/migrationHub")
    .withAutomaticReconnect()
    .build();

connection.on("MigrationStarted", (jobId: string) => {
    console.log("Migration started with job ID: " + jobId);
    window.location.href = "/Status/" + jobId;
});

connection.start().catch((err) => {
    console.error(err.toString());
});

const app = createApp(IndexApp, {
    connection: connection
});

app.mount('#app');
