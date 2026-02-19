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

            <div class="card mb-3">
                <div class="card-header">
                    <h5 class="mb-0">Migration Options</h5>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-3">
                            <label for="duplicateHandling" class="form-label">Duplicate Handling</label>
                            <select class="form-select" id="duplicateHandling" v-model="duplicateHandling">
                                <option value="Overwrite">Overwrite</option>
                                <option value="Skip">Skip</option>
                                <option value="CreateDuplicate">Create Duplicate</option>
                                <option value="Merge">Merge</option>
                            </select>
                        </div>
                        <div class="col-md-3">
                            <div class="form-check">
                                <input class="form-check-input" type="checkbox" id="importCollectedContacts" v-model="importCollectedContacts">
                                <label class="form-check-label" for="importCollectedContacts">
                                    Import Collected Contacts
                                </label>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <label for="importFolderName" class="form-label">Import Folder Name</label>
                            <input type="text" class="form-control" id="importFolderName" v-model="importFolderName">
                        </div>
                        <div class="col-md-3">
                            <div class="form-check">
                                <input class="form-check-input" type="checkbox" id="progressUpdates" v-model="progressUpdates">
                                <label class="form-check-label" for="progressUpdates">
                                    Progress Updates
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="text-center mb-3">
                <div class="form-check d-inline-block mb-3">
                    <input class="form-check-input" type="checkbox" id="tosCheckbox" v-model="tosAccepted">
                    <label class="form-check-label" for="tosCheckbox">
                        I agree to the <a href="/Tos" target="_blank">Terms of Service</a>
                    </label>
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

    const duplicateHandling = ref<string>('Skip');
    const importCollectedContacts = ref<boolean>(false);
    const importFolderName = ref<string>('');
    const progressUpdates = ref<boolean>(true);
    const tosAccepted = ref<boolean>(false);

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
        return isSourceAuthenticated.value && isDestAuthenticated.value && tosAccepted.value;
    });

    function handleAuthError(title: string, message: string) {
        feedbackModal.value?.alert({
            title,
            message,
            iconClass: 'x-circle'
        });
    }

    function startMigration() {
        if (!tosAccepted.value) {
            feedbackModal.value?.alert({
                title: 'Terms of Service',
                message: 'Please accept the Terms of Service before starting migration',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        const isSourceNextCloud = authState.sourceServerType.value === 'NextCloud';
        const isDestNextCloud = authState.destServerType.value === 'NextCloud';

        const sourceCredentials: types.Creds = {
            server: authState.sourceServer.value,
            user: authState.sourceUser.value,
            password: isSourceNextCloud ? authState.sourcePassword.value : '',
            oAuthToken: !isSourceNextCloud ? authState.sourceOAuthToken.value : null,
            useOAuth: !isSourceNextCloud,
            implementer: authState.sourceServerType.value
        };

        const destinationCredentials: types.Creds = {
            server: authState.destServer.value,
            user: authState.destUser.value,
            password: isDestNextCloud ? authState.destPassword.value : '',
            oAuthToken: !isDestNextCloud ? authState.destOAuthToken.value : null,
            useOAuth: !isDestNextCloud,
            implementer: authState.destServerType.value
        };

        const options: types.ContactMoverOptions = {
            duplicateHandling: duplicateHandling.value,
            deleteSource: false,
            importFolderName: importFolderName.value,
            importCollectedContacts: importCollectedContacts.value
        };

        const request: types.MigrationRequest = {
            $type: 'Contact',
            sourceCredentials: sourceCredentials,
            destinationCredentials: destinationCredentials,
            options: options,
            progressUpdates: progressUpdates.value
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
