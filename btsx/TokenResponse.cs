namespace Btsx
{
    /// <summary>
    /// C# binding for a token request response.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// OAuth Access token.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Seconds until expiry.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Refresh token.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Allowed scopes.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// Type of token.
        /// </summary>
        public string? TokenType { get; set; }

        public string? UserId { get; set; }
    }
}
