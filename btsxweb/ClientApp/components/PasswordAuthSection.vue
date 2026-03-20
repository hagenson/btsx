<template>
    <div>
        <div class="mb-3">
            <label :for="`${direction}Server`" class="form-label">Server</label>
            <input 
                type="text" 
                class="form-control" 
                :id="`${direction}Server`" 
                v-model="serverRef.value" 
                required
            >
        </div>
        <div class="mb-3">
            <label :for="`${direction}User`" class="form-label">Username</label>
            <input 
                type="text" 
                class="form-control" 
                :id="`${direction}User`" 
                v-model="userRef.value" 
                required
            >
        </div>
        <div class="mb-3">
            <label :for="`${direction}Password`" class="form-label">Password</label>
            <div class="input-group">
                <input 
                    :type="showPasswordRef.value ? 'text' : 'password'" 
                    class="form-control" 
                    :id="`${direction}Password`" 
                    v-model="passwordRef.value" 
                    required
                >
                <button 
                    class="btn btn-outline-secondary" 
                    type="button" 
                    @click="showPasswordRef.value = !showPasswordRef.value"
                >
                    <i :class="showPasswordRef.value ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </button>
            </div>
        </div>
        <div class="mb-3">
            <button 
                type="button" 
                class="btn btn-outline-primary" 
                @click="authMethods.authenticateServer(direction, migrationType)" 
                :disabled="authenticatingRef.value || !canAuthenticate"
            >
                <span v-if="authenticatingRef.value">
                    <span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                    Authenticating...
                </span>
                <span v-else>
                    <i class="bi bi-key"></i> Authenticate
                </span>
            </button>
            <div class="mt-2" v-if="authStatusRef.value">
                <span :class="authSuccessRef.value ? 'text-success' : 'text-danger'">
                    <i :class="authSuccessRef.value ? 'bi bi-check-circle' : 'bi bi-x-circle'"></i> {{ authStatusRef.value }}
                </span>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { computed, inject } from 'vue';
    import type { AuthState, AuthMethods } from '../composables/useAuth';
    import { AUTH_STATE_KEY, AUTH_METHODS_KEY } from '../composables/authKeys';

    interface Props {
        direction: 'source' | 'dest';
        migrationType: 'Mail' | 'Contacts';
    }

    const props = defineProps<Props>();

    const authState = inject(AUTH_STATE_KEY) as AuthState;
    const authMethods = inject(AUTH_METHODS_KEY) as AuthMethods;

    const serverRef = computed(() => 
        props.direction === 'source' ? authState.sourceServer : authState.destServer
    );

    const userRef = computed(() => 
        props.direction === 'source' ? authState.sourceUser : authState.destUser
    );

    const passwordRef = computed(() => 
        props.direction === 'source' ? authState.sourcePassword : authState.destPassword
    );

    const showPasswordRef = computed(() => 
        props.direction === 'source' ? authState.showSourcePassword : authState.showDestPassword
    );

    const authenticatingRef = computed(() => 
        props.direction === 'source' ? authState.sourceAuthenticating : authState.destAuthenticating
    );

    const authStatusRef = computed(() => 
        props.direction === 'source' ? authState.sourceAuthStatus : authState.destAuthStatus
    );

    const authSuccessRef = computed(() => 
        props.direction === 'source' ? authState.sourceAuthSuccess : authState.destAuthSuccess
    );

    const canAuthenticate = computed(() => {
        return serverRef.value.value && userRef.value.value && passwordRef.value.value;
    });
</script>
