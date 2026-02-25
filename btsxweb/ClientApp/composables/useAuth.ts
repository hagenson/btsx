import { ref, computed, watch, type Ref } from 'vue';
import type * as types from '../types/migration';

export interface AuthState {
    sourceServerType: Ref<string>;
    destServerType: Ref<string>;
    sourceServer: Ref<string>;
    destServer: Ref<string>;
    sourceUser: Ref<string>;
    destUser: Ref<string>;
    sourcePassword: Ref<string>;
    destPassword: Ref<string>;
    sourceOAuthToken: Ref<string>;
    destOAuthToken: Ref<string>;
    sourceOAuthStatus: Ref<string>;
    destOAuthStatus: Ref<string>;
    sourceAuthStatus: Ref<string>;
    destAuthStatus: Ref<string>;
    sourceAuthSuccess: Ref<boolean>;
    destAuthSuccess: Ref<boolean>;
    sourceAuthenticating: Ref<boolean>;
    destAuthenticating: Ref<boolean>;
    showSourcePassword: Ref<boolean>;
    showDestPassword: Ref<boolean>;
}

export interface AuthMethods {
    authenticateServer: (type: 'source' | 'dest', migrationType: 'Mail' | 'Contacts') => Promise<void>;
    invokeOAuth: (direction: 'Source' | 'Destination', migrationType: 'Mail' | 'Contacts', onError: (title: string, message: string) => void) => Promise<void>;
    resetSourceAuth: () => void;
    resetDestAuth: () => void;
}

export function useAuth(): AuthState & AuthMethods {
    const sourceServerType = ref<string>('');
    const sourceServer = ref<string>('');
    const sourceUser = ref<string>('');
    const sourcePassword = ref<string>('');
    const sourceOAuthToken = ref<string>('');
    const sourceOAuthStatus = ref<string>('');
    const sourceAuthStatus = ref<string>('');
    const sourceAuthSuccess = ref<boolean>(false);
    const sourceAuthenticating = ref<boolean>(false);

    const destServerType = ref<string>('');
    const destServer = ref<string>('');
    const destUser = ref<string>('');
    const destPassword = ref<string>('');
    const destOAuthToken = ref<string>('');
    const destOAuthStatus = ref<string>('');
    const destAuthStatus = ref<string>('');
    const destAuthSuccess = ref<boolean>(false);
    const destAuthenticating = ref<boolean>(false);

    const showSourcePassword = ref<boolean>(false);
    const showDestPassword = ref<boolean>(false);

    let implementer: string | undefined = undefined;
    let authDirection: string | undefined = undefined;

    watch(sourceServerType, (newType) => {
        if (newType === 'Google') {
            sourceServer.value = 'imap.gmail.com';
        } else {
            sourceServer.value = '';
            sourceUser.value = '';
            sourceOAuthToken.value = '';
            sourceOAuthStatus.value = '';
        }
        sourceAuthStatus.value = '';
        sourceAuthSuccess.value = false;
    });

    watch(destServerType, (newType) => {
        if (newType === 'Google') {
            destServer.value = 'imap.gmail.com';
        } else {
            destServer.value = '';
            destUser.value = '';
            destOAuthToken.value = '';
            destOAuthStatus.value = '';
        }
        destAuthStatus.value = '';
        destAuthSuccess.value = false;
    });

    watch([sourceServer, sourceUser, sourcePassword], () => {
            sourceAuthStatus.value = '';
            sourceAuthSuccess.value = false;
    });

    watch([destServer, destUser, destPassword], () => {
            destAuthStatus.value = '';
            destAuthSuccess.value = false;
    });

    async function authenticateServer(type: 'source' | 'dest', migrationType: 'Mail' | 'Contacts'): Promise<void> {
        if (type === 'source') {
            sourceAuthenticating.value = true;
            sourceAuthStatus.value = '';
            sourceAuthSuccess.value = false;
        } else {
            destAuthenticating.value = true;
            destAuthStatus.value = '';
            destAuthSuccess.value = false;
        }

        try {
            const request: types.TestAuthRequest = {
                server: type === 'source' ? sourceServer.value : destServer.value,
                user: type === 'source' ? sourceUser.value : destUser.value,
                password: type === 'source' ? sourcePassword.value : destPassword.value,
                implementer: type === 'source' ? sourceServerType.value : destServerType.value,
                migrationType: migrationType
            };

            const response = await fetch('/?handler=TestAuth', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(request)
            });

            if (!response.ok) {
                const error = await response.text();
                if (type === 'source') {
                    sourceAuthStatus.value = 'Authentication failed';
                    sourceAuthSuccess.value = false;
                } else {
                    destAuthStatus.value = 'Authentication failed';
                    destAuthSuccess.value = false;
                }
                return;
            }

            const data = await response.json();

            if (type === 'source') {
                sourceAuthStatus.value = data.message;
                sourceAuthSuccess.value = data.success;
            } else {
                destAuthStatus.value = data.message;
                destAuthSuccess.value = data.success;
            }
        } catch (error: any) {
            console.error('Error testing authentication:', error);
            if (type === 'source') {
                sourceAuthStatus.value = 'Error: ' + error.message;
                sourceAuthSuccess.value = false;
            } else {
                destAuthStatus.value = 'Error: ' + error.message;
                destAuthSuccess.value = false;
            }
        } finally {
            if (type === 'source') {
                sourceAuthenticating.value = false;
            } else {
                destAuthenticating.value = false;
            }
        }
    }

    async function invokeOAuth(direction: 'Source' | 'Destination', migrationType: 'Mail' | 'Contacts', onError: (title: string, message: string) => void): Promise<void> {
        try {
            implementer = direction === 'Source'
                ? sourceServerType.value
                : destServerType.value;
            authDirection = direction;
            const response = await fetch(`/?handler=OAuthUrl&direction=${direction}&implementer=${implementer}&migrationType=${migrationType}`);
            if (!response.ok) {
                const error = await response.text();
                onError('Error', 'Error: ' + error);
                return;
            }

            const data = await response.json();

            const width = 600;
            const height = 700;
            const left = (screen.width / 2) - (width / 2);
            const top = (screen.height / 2) - (height / 2);

            const authWindow = window.open(
                data.authUrl,
                `Authenticate with ${direction} service`,
                `width=${width},height=${height},left=${left},top=${top}`
            );

        } catch (error: any) {
            console.error('Error initiating OAuth:', error);
            onError('Error', 'Error initiating OAuth: ' + error.message);
        }
    }

    function resetSourceAuth(): void {
        sourceAuthStatus.value = '';
        sourceAuthSuccess.value = false;
        sourceOAuthToken.value = '';
        sourceOAuthStatus.value = '';
    }

    function resetDestAuth(): void {
        destAuthStatus.value = '';
        destAuthSuccess.value = false;
        destOAuthToken.value = '';
        destOAuthStatus.value = '';
    }

    window.addEventListener('message', function (event) {
        if (event.origin !== window.location.origin) {
            return;
        }

        if (event.data.type === 'oauth-success') {
            if (event.data.serverType === implementer) {
                if (authDirection === 'Source') {
                    sourceOAuthToken.value = event.data.token;
                    sourceUser.value = event.data.email || '';
                    sourceOAuthStatus.value = 'Authenticated';
                } else {
                    destOAuthToken.value = event.data.token;
                    destUser.value = event.data.email || '';
                    destOAuthStatus.value = 'Authenticated';
                }
            }
        }
    }, false);

    return {
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
    };
}
