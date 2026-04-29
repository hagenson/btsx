<template>
    <div class="container mt-4">
        <h3 class="mb-4">Mail Migration Tool</h3>

        <div id="migrationForm">
            <div class="row">
                <div class="col-md-6">
                    <ServerCredentialCard
                        direction="source"
                        title="Source Account"
                        headerClass="bg-primary"
                        :serverTypeOptions="[{value: 'generic', label: 'IMAP'}, {value: 'Google', label: 'GMail'}]"
                        migrationType="Mail"
                        :useOAuthPredicate="(serverType: string) => serverType !== 'generic'"
                        :onAuthError="handleAuthError"
                    />
                </div>

                <div class="col-md-6">
                    <ServerCredentialCard
                        direction="dest"
                        title="Destination Account"
                        headerClass="bg-success"
                        :serverTypeOptions="[{value: 'generic', label: 'IMAP'}, {value: 'Google', label: 'GMail'}]"
                        migrationType="Mail"
                        :useOAuthPredicate="(serverType: string) => serverType !== 'generic'"
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
                        <div class="col-md-12">
                            <div class="d-inline-block border-start border-end border-1 p-2">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" id="progressUpdates" v-model="progressUpdates">
                                    <label class="form-check-label" for="progressUpdates">
                                        Progress Updates
                                    </label>
                                </div>
                            </div>
                            <div class="d-inline-block border-start border-end border-1 p-2" style="padding-top: 0 !important; padding-bottom: 0 !important">
                                <div class="d-inline-block">
                                    <label for="duplicateHandling" class="form-label">Duplicate Handling</label>
                                </div>
                                <div class="d-inline-block">&nbsp;&nbsp;</div>
                                <div class="d-inline-block">
                                    <select class="form-select" id="duplicateHandling" v-model="duplicateHandling">
                                        <option value="Overwrite">Overwrite</option>
                                        <option value="Skip">Skip</option>
                                        <option value="CreateDuplicate">Create Duplicate</option>
                                    </select>
                                </div>
                            </div>
                            <div class="d-inline-block border-start border-end border-1 p-2">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" id="deleteSource" v-model="deleteSource">
                                    <label class="form-check-label" for="deleteSource">
                                        Delete Source
                                    </label>
                                </div>
                            </div>
                            <div class="d-inline-block border-start border-end border-1 p-2">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" id="foldersOnly" v-model="foldersOnly">
                                    <label class="form-check-label" for="foldersOnly">
                                        Folders Only
                                    </label>
                                </div>
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
                <button id="startBtn" class="btn btn-primary btn-lg" @click="startMigration" :disabled="!canStartMigration">
                    <i class="bi bi-play-fill"></i> Start Migration
                </button>
            </div>
        </div>

        <FeedbackModal ref="feedbackModal" />
    </div>
</template>

<script setup lang="ts">
    import { ref, computed, inject } from 'vue';
    import type * as signalR from '@microsoft/signalr';
    import type * as types from '../types/migration';
    import FeedbackModal from './FeedbackModal.vue';
    import ServerCredentialCard from './ServerCredentialCard.vue';
    import { MigrationRequest } from '../types/migration';
    import type { AuthState, AuthMethods } from '../composables/useAuth';
    import { AUTH_STATE_KEY, AUTH_METHODS_KEY } from '../composables/authKeys';

    interface Props {
        connection: signalR.HubConnection;
    }

    const props = defineProps<Props>();

    const authState = inject(AUTH_STATE_KEY) as AuthState;
    const authMethods = inject(AUTH_METHODS_KEY) as AuthMethods;

    const deleteSource = ref<boolean>(false);
    const foldersOnly = ref<boolean>(false);
    const progressUpdates = ref<boolean>(true);
    const duplicateHandling = ref<string>('Overwrite');

    const tosAccepted = ref<boolean>(false);

    const isStarting = ref<boolean>(false);

    const feedbackModal = ref<InstanceType<typeof FeedbackModal> | null>(null);

    const isSourceAuthenticated = computed(() => {
        const isGeneric = authState.sourceServerType.value === 'generic';
        if (!isGeneric) {
            return !!authState.sourceOAuthToken.value;
        } else {
            return authState.sourceAuthSuccess.value;
        }
    });

    const isDestAuthenticated = computed(() => {
        const isGeneric = authState.destServerType.value === 'generic';
        if (!isGeneric) {
            return !!authState.destOAuthToken.value;
        } else {
            return authState.destAuthSuccess.value;
        }
    });

    const canStartMigration = computed(() => {
        return !isStarting.value && isSourceAuthenticated.value && isDestAuthenticated.value && tosAccepted.value;
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

        const sourceUseOAuth = authState.sourceServerType.value !== 'generic';
        const destUseOAuth = authState.destServerType.value !== 'generic';

        const request: MigrationRequest = {
            $type: 'Mail',
            destinationCredentials: {
                server: authState.destServer.value,
                user: authState.destUser.value,
                password: destUseOAuth ? "" : authState.destPassword.value,
                oAuthToken: destUseOAuth ? authState.destOAuthToken.value : null,
                useOAuth: destUseOAuth,
                implementer: authState.destServerType.value
            },
            sourceCredentials: {
                server: authState.sourceServer.value,
                user: authState.sourceUser.value,
                password: sourceUseOAuth ? "" : authState.sourcePassword.value,
                oAuthToken: sourceUseOAuth ? authState.sourceOAuthToken.value : null,
                useOAuth: sourceUseOAuth,
                implementer: authState.sourceServerType.value
            },
            options: {
                deleteSource: deleteSource.value,
                foldersOnly: foldersOnly.value,
                duplicateHandling: duplicateHandling.value
            },
            
            progressUpdates: progressUpdates.value,
        };

        if (!request.sourceCredentials?.server) {
            feedbackModal.value?.alert({
                title: 'Validation Error',
                message: 'Please fill in source server',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        if (!sourceUseOAuth && !request.sourceCredentials?.user) {
            feedbackModal.value?.alert({
                title: 'Validation Error',
                message: 'Please fill in source username',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        if (!sourceUseOAuth && !request.sourceCredentials?.password) {
            feedbackModal.value?.alert({
                title: 'Validation Error',
                message: 'Please fill in source password',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        if (sourceUseOAuth && !request.sourceCredentials?.oAuthToken) {
            feedbackModal.value?.alert({
                title: 'Validation Error',
                message: 'Please authenticate with Google for source server',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        if (!request.destinationCredentials.server) {
            feedbackModal.value?.alert({
                title: 'Validation Error',
                message: 'Please fill in destination server',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        if (!destUseOAuth && !request.destinationCredentials?.user) {
            feedbackModal.value?.alert({
                title: 'Validation Error',
                message: 'Please fill in destination username',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        if (!destUseOAuth && !request.destinationCredentials?.password) {
            feedbackModal.value?.alert({
                title: 'Validation Error',
                message: 'Please fill in destination password',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        if (destUseOAuth && !request.destinationCredentials?.oAuthToken) {
            feedbackModal.value?.alert({
                title: 'Validation Error',
                message: 'Please authenticate with Google for destination server',
                iconClass: 'exclamation-triangle'
            });
            return;
        }

        isStarting.value = true;

        props.connection.invoke("StartMigration", request).catch((err: Error) => {
            console.error(err.toString());
            feedbackModal.value?.alert({
                title: 'Error',
                message: 'Error starting migration: ' + err.toString(),
                iconClass: 'x-circle'
            });
            isStarting.value = false;
        });
    }
</script>
