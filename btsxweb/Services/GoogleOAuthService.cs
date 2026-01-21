using btsxweb.Services;
using BtsxWeb.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

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
            IHttpClientFactory httpClientFactory,
            ILogger<GoogleOAuthService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;

            clientId = googleOAuthSettings.Value.ClientId;
            clientSecret = googleOAuthSettings.Value.ClientSecret;
            redirectUri = googleOAuthSettings.Value.RedirectUri;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                throw new InvalidOperationException("Google OAuth is not configured properly");
        }

        /// <summary>
        /// Gets the email address for the account that created the access token.
        /// </summary>
        /// <param name="token">OAuth access token to get the email address for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The account's email address.</returns>
        public async Task<string?> GetEmailForTokenAsync(string token, CancellationToken cancellationToken)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                var userInfoRequest = new HttpRequestMessage(HttpMethod.Get,
                "https://www.googleapis.com/oauth2/v2/userinfo");
                userInfoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

                var userInfoResponse = await httpClient.SendAsync(userInfoRequest);
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userInfoResponse.Content.ReadAsStringAsync();
                    logger.LogError("Get email request failed: {Error}", errorContent);
                    throw new InvalidOperationException(errorContent);
                }

                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<UserInfoResponse>(userInfoContent);
                return userInfo?.email;
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
            using (var httpClient = httpClientFactory.CreateClient())
            {
                var tokenRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri ?? "" },
                { "grant_type", "authorization_code" }
            };

                var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(tokenRequest));

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("OAuth token exchange failed: {Error}", errorContent);
                    throw new InvalidOperationException(errorContent);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TokenResponse>(responseContent);
                if (result == null)
                    throw new InvalidOperationException("token response could not be deserialised.");

                return result;
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
                var httpClient = httpClientFactory.CreateClient();
                var revokeRequest = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "token", token }
                });

                var response = await httpClient.PostAsync("https://oauth2.googleapis.com/revoke", revokeRequest);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Successfully revoked OAuth token");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogWarning("Failed to revoke OAuth token. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Exception occurred while revoking OAuth token");
                return false;
            }
        }

        private readonly string clientId;

        private readonly string clientSecret;

        private readonly IHttpClientFactory httpClientFactory;

        private readonly ILogger<GoogleOAuthService> logger;

        private readonly string redirectUri;

        private class UserInfoResponse
        {
            public string? email { get; set; }
        }
    }
}