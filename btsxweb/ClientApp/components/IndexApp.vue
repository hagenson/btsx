<template>
    <h1>BTSX Data Mover</h1>
    <div class="container mt-4">
        <div class="mb-4">
            <label for="migrationType" class="form-label">Choose the migration type</label>
            <select class="form-select" id="migrationType" v-model="migrationType">
                <option value="Mail">Mail</option>
                <option value="Contacts">Contacts</option>
            </select>
        </div>
    </div>
    
    <MailApp v-if="migrationType === 'Mail'" :connection="connection" />
    <ContactsApp v-else-if="migrationType === 'Contacts'" :connection="connection" />
</template>

<script setup lang="ts">
    import { ref, provide } from 'vue';
    import type * as signalR from '@microsoft/signalr';
    import MailApp from './MailApp.vue';
    import ContactsApp from './ContactsApp.vue';
    import { useAuth } from '../composables/useAuth';
    import { AUTH_STATE_KEY, AUTH_METHODS_KEY } from '../composables/authKeys';

    interface Props {
        connection: signalR.HubConnection;
    }

    const props = defineProps<Props>();

    const migrationType = ref<string>('Mail');

    const {
        sourceServerType,
        destServerType,
        sourceServer,
        destServer,
        sourceUser,
        destUser,
        sourcePassword,
        destPassword,
        sourceOAuthToken,
        destOAuthToken,
        sourceOAuthStatus,
        destOAuthStatus,
        sourceAuthStatus,
        destAuthStatus,
        sourceAuthSuccess,
        destAuthSuccess,
        sourceAuthenticating,
        destAuthenticating,
        showSourcePassword,
        showDestPassword,
        authenticateServer,
        invokeOAuth,
        resetSourceAuth,
        resetDestAuth
    } = useAuth();

    provide(AUTH_STATE_KEY, {
        sourceServerType,
        destServerType,
        sourceServer,
        destServer,
        sourceUser,
        destUser,
        sourcePassword,
        destPassword,
        sourceOAuthToken,
        destOAuthToken,
        sourceOAuthStatus,
        destOAuthStatus,
        sourceAuthStatus,
        destAuthStatus,
        sourceAuthSuccess,
        destAuthSuccess,
        sourceAuthenticating,
        destAuthenticating,
        showSourcePassword,
        showDestPassword
    });

    provide(AUTH_METHODS_KEY, {
        authenticateServer,
        invokeOAuth,
        resetSourceAuth,
        resetDestAuth
    });
</script>
