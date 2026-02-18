<template>
    <div class="container mt-4">
        <h1 class="mb-4">Contact Migration Tool</h1>

        <div id="contactsForm">
            <div class="row">
                <div class="col-md-6">
                    <ServerCredentialCard
                        direction="source"
                        title="Source Account"
                        headerClass="bg-primary"
                        :serverTypeOptions="[{value: 'NextCloud', label: 'NextCloud'}, {value: 'Google', label: 'Google'}]"
                        migrationType="Contacts"
                        :useOAuthPredicate="(serverType) => serverType !== 'NextCloud'"
                        :onAuthError="handleAuthError"
                    />
                </div>

                <div class="col-md-6">
                    <ServerCredentialCard
                        direction="dest"
                        title="Destination Account"
                        headerClass="bg-success"
                        :serverTypeOptions="[{value: 'NextCloud', label: 'NextCloud'}, {value: 'Google', label: 'Google'}]"
                        migrationType="Contacts"
                        :useOAuthPredicate="(serverType) => serverType !== 'NextCloud'"
                        :onAuthError="handleAuthError"
                    />
                </div>
            </div>

            <div class="text-center mb-3">
                <button id="migrateBtn" class="btn btn-primary btn-lg" @click="startMigration" :disabled="!canStartMigration">
                    <i class="bi bi-arrow-left-right"></i> Migrate Contacts
                </button>
            </div>
        </div>
    </div>
    <FeedbackModal ref="feedbackModal" />
</template>

<script setup lang="ts">
    import { ref, computed, inject } from 'vue';
    import type * as signalR from '@microsoft/signalr';
    import type * as types from '../types/migration';
    import FeedbackModal from './FeedbackModal.vue';
    import ServerCredentialCard from './ServerCredentialCard.vue';
    import type { AuthState, AuthMethods } from '../composables/useAuth';
    import { AUTH_STATE_KEY, AUTH_METHODS_KEY } from '../composables/authKeys';

    interface Props {
        connection: signalR.HubConnection;
    }

    const props = defineProps<Props>();

    const authState = inject(AUTH_STATE_KEY) as AuthState;
    const authMethods = inject(AUTH_METHODS_KEY) as AuthMethods;

    const feedbackModal = ref<InstanceType<typeof FeedbackModal> | null>(null);

    const isSourceAuthenticated = computed(() => {
        if (authState.sourceServerType.value !== 'NextCloud') {
            return !!authState.sourceOAuthToken.value;
        } else {
            return authState.sourceAuthSuccess.value;
        }
    });

    const isDestAuthenticated = computed(() => {
        if (authState.destServerType.value !== 'NextCloud') {
            return !!authState.destOAuthToken.value;
        } else {
            return authState.destAuthSuccess.value;
        }
    });

    const canStartMigration = computed(() => {
        return isSourceAuthenticated.value && isDestAuthenticated.value;
    });

    function handleAuthError(title: string, message: string) {
        feedbackModal.value?.alert({
            title,
            message,
            iconClass: 'x-circle'
        });
    }

    function startMigration() {
        const isSourceNextCloud = authState.sourceServerType.value === 'NextCloud';
        const isDestNextCloud = authState.destServerType.value === 'NextCloud';

        const sourceCredentials: types.Creds = {
            server: authState.sourceServer.value,
            user: authState.sourceUser.value,
            password: isSourceNextCloud ? authState.sourcePassword.value : '',
            oAuthToken: !isSourceNextCloud ? authState.sourceOAuthToken.value : null,
            useOAuth: !isSourceNextCloud,
            implementor: authState.sourceServerType.value
        };

        const destinationCredentials: types.Creds = {
            server: authState.destServer.value,
            user: authState.destUser.value,
            password: isDestNextCloud ? authState.destPassword.value : '',
            oAuthToken: !isDestNextCloud ? authState.destOAuthToken.value : null,
            useOAuth: !isDestNextCloud,
            implementor: authState.destServerType.value
        };

        const options: types.ContactMoverOptions = {
            replaceExisting: false,
            deleteSource: false,
            importFolderName: '',
            importCollectedContacts: false
        };

        const request: types.MigrationRequest = {
            $type: 'Contact',
            sourceCredentials: sourceCredentials,
            destinationCredentials: destinationCredentials,
            options: options,
            progressUpdates: false
        };

        props.connection.invoke('StartMigration', request).catch((err: Error) => {
            console.error(err.toString());
            feedbackModal.value?.alert({
                title: 'Error',
                message: 'Error starting migration: ' + err.toString(),
                iconClass: 'x-circle'
            });
        });

        console.log('Starting contact migration...');
    }
</script>
