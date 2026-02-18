using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GoogleAuthResponses = Google.Apis.Auth.OAuth2.Responses;

namespace Btsx.Google
{
    /// <summary>
    /// Abstracts the Google API service calls we need.
    /// </summary>
    public class GoogleOAuthService: IOAuthService
    {
        /// <summary>
        /// Initialises the service.
        /// </summary>
        public GoogleOAuthService(
            IOptions<GoogleOAuthSettings> googleOAuthSettings,
            ILogger<GoogleOAuthService> logger)
        {
            this.logger = logger;

            clientId = googleOAuthSettings.Value.ClientId;
            clientSecret = googleOAuthSettings.Value.ClientSecret;
            redirectUri = string.Format(googleOAuthSettings.Value.RedirectUri, "Google");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                throw new InvalidOperationException("Google OAuth is not configured properly");

            flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { "openid", "email", "https://mail.google.com/", "https://www.googleapis.com/auth/contacts" }
            });
        }

        /// <summary>
        /// Requests an OAuth token from the google API.
        /// </summary>
        /// <param name="code">Code provided byu the front-end authentication step.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Deserialised token request response.</returns>
        public async Task<TokenResponse> RequestTokenAsync(
            string code,
            CancellationToken cancellationToken)
        {
            try
            {
                var tokenResponse = await flow.ExchangeCodeForTokenAsync(
                    string.Empty,
                    code,
                    redirectUri,
                    cancellationToken);

                var credential = new UserCredential(flow, string.Empty, tokenResponse);

                // Use Google's Oauth2 API to request the userinfo (which includes email when scope includes 'email' or 'openid')
                using var oauth2 = new Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "BtsxWeb"
                });

                var userInfo = await oauth2.Userinfo.Get().ExecuteAsync(cancellationToken);

                return new TokenResponse
                {
                    AccessToken = tokenResponse.AccessToken,
                    ExpiryDate = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 0),
                    RefreshToken = tokenResponse.RefreshToken,
                    Scope = tokenResponse.Scope,
                    TokenType = tokenResponse.TokenType,
                    UserId = userInfo.Email,
                };
            }
            catch (GoogleAuthResponses.TokenResponseException ex)
            {
                logger.LogError(ex, "OAuth token exchange failed: {Error}", ex.Error?.Error);
                throw new InvalidOperationException($"OAuth token exchange failed: {ex.Error?.Error}", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OAuth token exchange failed");
                throw new InvalidOperationException("OAuth token exchange failed", ex);
            }
        }

        /// <summary>
        /// Revokes a Google OAuth token.
        /// </summary>
        /// <param name="token">Token to revoke.</param>
        /// <returns>True if the token was revoked.</returns>
        public async Task<bool> RevokeTokenAsync(string? token, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return true;
            }

            try
            {
                await flow.RevokeTokenAsync(string.Empty, token, cancellationToken);
                logger.LogInformation("Successfully revoked OAuth token");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Exception occurred while revoking OAuth token");
                return false;
            }
        }

        public string GetAuthUrl(MigrationType type, MigrationDirection direction, string state)
        {
            var scope = type switch
            {
                MigrationType.Mail => "https://mail.google.com/ https://www.googleapis.com/auth/userinfo.email",
                MigrationType.Contacts => "https://www.googleapis.com/auth/contacts https://www.googleapis.com/auth/contacts.other.readonly https://www.googleapis.com/auth/userinfo.email",
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported migration type: {type}")
            };
                
            scope = Uri.EscapeDataString(scope);
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={Uri.EscapeDataString(clientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}&access_type=offline&prompt=consent&state={state}";
            return authUrl;
        }


        private readonly string clientId;

        private readonly string clientSecret;

        private readonly GoogleAuthorizationCodeFlow flow;

        private readonly ILogger<GoogleOAuthService> logger;

        private readonly string redirectUri;
    }
}