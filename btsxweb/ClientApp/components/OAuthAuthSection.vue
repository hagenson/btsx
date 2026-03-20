<template>
    <div>
        <div class="mb-3">
            <label :for="`${direction}Server`" class="form-label">Server</label>
            <input 
                type="text" 
                class="form-control bg-secondary bg-opacity-25" 
                :id="`${direction}Server`" 
                v-model="serverRef.value" 
                readonly 
                required
            >
        </div>
        <div class="mb-3">
            <label :for="`${direction}User`" class="form-label">Username</label>
            <input 
                type="text" 
                class="form-control bg-secondary bg-opacity-25" 
                :id="`${direction}User`" 
                v-model="userRef.value" 
                readonly 
                required
            >
        </div>
        <div class="mb-3" v-if="serverType">
            <button 
                type="button" 
                class="btn btn-outline-primary" 
                @click="handleOAuth"
            >
                <i class="bi bi-google"></i> Authenticate with {{ serverType }}
            </button>
            <div class="mt-2" v-if="oauthStatusRef.value">
                <span class="text-success">
                    <i class="bi bi-check-circle"></i> {{ oauthStatusRef.value }}
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
        direction: 'Source' | 'Destination';
        serverType: string;
        migrationType: 'Mail' | 'Contacts';
        onAuthError: (title: string, message: string) => void;
    }

    const props = defineProps<Props>();

    const authState = inject(AUTH_STATE_KEY) as AuthState;
    const authMethods = inject(AUTH_METHODS_KEY) as AuthMethods;

    const serverRef = computed(() => 
        props.direction === 'Source' ? authState.sourceServer : authState.destServer
    );

    const userRef = computed(() => 
        props.direction === 'Source' ? authState.sourceUser : authState.destUser
    );

    const oauthStatusRef = computed(() => 
        props.direction === 'Source' ? authState.sourceOAuthStatus : authState.destOAuthStatus
    );

    function handleOAuth() {
        authMethods.invokeOAuth(props.direction, props.migrationType, props.onAuthError);
    }
</script>
