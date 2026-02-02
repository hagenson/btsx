import { createApp } from 'vue';
import * as signalR from '@microsoft/signalr';
import ContactStatusApp from './components/ContactStatusApp.vue';

const jobId = (document.getElementById('jobId') as HTMLInputElement)?.value || '';

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/migrationHub")
    .withAutomaticReconnect()
    .build();

connection.onreconnected(async () => {
    console.log('SignalR reconnected');
    await connection.invoke("JoinJobGroup", jobId);
});

connection.start()
    .then(() => {
        component.loadJobInfo();
        return connection.invoke("JoinJobGroup", jobId);
    })
    .catch((err) => {
        console.error(err.toString());
    });

const app = createApp(ContactStatusApp, {
    connection: connection,
    jobId: jobId
});

const component: any = app.mount('#app');
