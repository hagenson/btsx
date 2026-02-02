using btsxweb.Services;
using BtsxWeb.Models;
using Microsoft.Extensions.Options;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using GoogleAuthResponses = Google.Apis.Auth.OAuth2.Responses;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Abstracts the Google API service calls we need.
    /// </summary>
    public class GoogleOAuthService
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
            redirectUri = googleOAuthSettings.Value.RedirectUri;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                throw new InvalidOperationException("Google OAuth is not configured properly");

            flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { "openid", "email", "https://mail.google.com/", "https://www.googleapis.com/auth/contacts.readonly" }
            });
        }

        /// <summary>
        /// Gets the email address for the account that created the access token.
        /// </summary>
        /// <param name="token">OAuth access token to get the email address for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The account's email address.</returns>
        public async Task<string?> GetEmailForTokenAsync(string token, CancellationToken cancellationToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);
                return payload.Email;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to validate token and get email");
                throw new InvalidOperationException("Failed to validate token and get email", ex);
            }
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

                return new TokenResponse
                {
                    access_token = tokenResponse.AccessToken,
                    expires_in = (int)(tokenResponse.ExpiresInSeconds ?? 0),
                    refresh_token = tokenResponse.RefreshToken,
                    scope = tokenResponse.Scope,
                    token_type = tokenResponse.TokenType
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
        /// Revokes all OSuath tokens for a migration job.
        /// </summary>
        /// <param name="job">Job to revoke the tokens for.</param>
        /// <returns>Awaitable task</returns>
        public async Task<(bool sourceRevoked, bool destRevoked)> RevokeJobTokensAsync(MigrationJob job)
        {
            var sourceRevoked = true;
            var destRevoked = true;

            if (job.Request.SourceUseOAuth && !string.IsNullOrWhiteSpace(job.Request.SourceOAuthToken))
            {
                logger.LogInformation("Revoking source OAuth token for job {JobId}", job.JobId);
                sourceRevoked = await RevokeTokenAsync(job.Request.SourceOAuthToken);
            }

            if (job.Request.DestUseOAuth && !string.IsNullOrWhiteSpace(job.Request.DestOAuthToken))
            {
                logger.LogInformation("Revoking destination OAuth token for job {JobId}", job.JobId);
                destRevoked = await RevokeTokenAsync(job.Request.DestOAuthToken);
            }

            return (sourceRevoked, destRevoked);
        }

        /// <summary>
        /// Revokes a Google OAuth token.
        /// </summary>
        /// <param name="token">Token to revoke.</param>
        /// <returns>True if the token was revoked.</returns>
        public async Task<bool> RevokeTokenAsync(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return true;
            }

            try
            {
                await flow.RevokeTokenAsync(string.Empty, token, CancellationToken.None);
                logger.LogInformation("Successfully revoked OAuth token");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Exception occurred while revoking OAuth token");
                return false;
            }
        }

        private readonly string clientId;

        private readonly string clientSecret;

        private readonly GoogleAuthorizationCodeFlow flow;

        private readonly ILogger<GoogleOAuthService> logger;

        private readonly string redirectUri;
    }
}
