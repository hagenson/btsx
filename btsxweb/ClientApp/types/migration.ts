export interface Creds {
    server: string;
    user: string;
    password: string;
    oAuthToken: string | null;
    useOAuth: boolean;
    implementor: string;
}
export interface MigrationRequest {
    $type: string;
    sourceCredentials: Creds;
    destinationCredentials: Creds;
    options: any;
    progressUpdates: boolean;
}

export interface OAuthResponse {
    authUrl: string;
}

export interface OAuthMessageData {
    type: 'oauth-success' | 'oauth-error';
    token?: string;
    email?: string;
    serverType?: string;
    error?: string;
}

export interface ContactMoverOptions {
    duplicateHandling: string;
    deleteSource: boolean;
    importFolderName: string;
    importCollectedContacts: boolean;
}

export interface TestAuthRequest {
    password: string;
    server: string;
    user: string;
    implementer: string;
    migrationType: 'Mail' | 'Contacts';
}