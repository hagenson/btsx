<template>
    <div class="card mb-3">
        <div class="card-header text-white" :class="headerClass">
            <h5 class="mb-0">{{ title }}</h5>
        </div>
        <div class="card-body">
            <div class="mb-3">
                <label :for="`${direction}ServerType`" class="form-label">Server Type</label>
                <select 
                    class="form-select" 
                    :id="`${direction}ServerType`" 
                    v-model="serverTypeRef.value"
                >
                    <option 
                        v-for="option in serverTypeOptions" 
                        :key="option.value" 
                        :value="option.value"
                    >
                        {{ option.label }}
                    </option>
                </select>
            </div>
            <PasswordAuthSection 
                v-if="!shouldUseOAuth" 
                :direction="direction" 
                :migrationType="migrationType"
            />
            <OAuthAuthSection 
                v-else 
                :direction="directionCapitalized" 
                :serverType="serverTypeRef.value" 
                :onAuthError="onAuthError"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
    import { computed, inject } from 'vue';
    import type { AuthState } from '../composables/useAuth';
    import { AUTH_STATE_KEY } from '../composables/authKeys';
    import PasswordAuthSection from './PasswordAuthSection.vue';
    import OAuthAuthSection from './OAuthAuthSection.vue';

    interface ServerTypeOption {
        value: string;
        label: string;
    }

    interface Props {
        direction: 'source' | 'dest';
        title: string;
        headerClass: string;
        serverTypeOptions: ServerTypeOption[];
        migrationType: 'Mail' | 'Contacts';
        useOAuthPredicate: (serverType: string) => boolean;
        onAuthError: (title: string, message: string) => void;
    }

    const props = defineProps<Props>();

    const authState = inject(AUTH_STATE_KEY) as AuthState;

    const serverTypeRef = computed(() => 
        props.direction === 'source' ? authState.sourceServerType : authState.destServerType
    );

    const shouldUseOAuth = computed(() => 
        props.useOAuthPredicate(serverTypeRef.value.value)
    );

    const directionCapitalized = computed(() => 
        props.direction === 'source' ? 'Source' : 'Destination'
    );
</script>
