namespace Btsxweb.Services
{
    /// <summary>
    /// C# binding for a token request response.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// OAuth Access token.
        /// </summary>
        public string? access_token { get; set; }

        /// <summary>
        /// Seconds until expiry.
        /// </summary>
        public int expires_in { get; set; }

        /// <summary>
        /// Refresh token.
        /// </summary>
        public string? refresh_token { get; set; }

        /// <summary>
        /// Allowed scopes.
        /// </summary>
        public string? scope { get; set; }

        /// <summary>
        /// Type of token.
        /// </summary>
        public string? token_type { get; set; }

        public string? user_id { get; set; }
    }
}
