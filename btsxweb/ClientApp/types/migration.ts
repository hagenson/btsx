export interface MigrationRequest {
  sourceServer: string;
  sourceUser: string;
  sourcePassword: string;
  sourceOAuthToken: string | null;
  sourceUseOAuth: boolean;
  destServer: string;
  destUser: string;
  destPassword: string;
  destOAuthToken: string | null;
  destUseOAuth: boolean;
  deleteSource: boolean;
  foldersOnly: boolean;
  progressUpdates: boolean;
  replaceExisting: boolean;
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
