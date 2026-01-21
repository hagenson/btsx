namespace Btsx
{
    /// <summary>
    /// Encapsulates email account credentials.
    /// </summary>
    public class Creds
    {
        /// <summary>
        /// OAuth token obtains from an OAuth provider.
        /// </summary>
        public string? OAuthToken { get; set; }

        /// <summary>
        /// Account password.
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// Email server host name.
        /// </summary>
        public string Server { get; set; } = "";

        /// <summary>
        /// True to use the OAuth token to authenticate, false to use the Password.
        /// </summary>
        public bool UseOAuth { get; set; }

        /// <summary>
        /// User name for the mail account.
        /// </summary>
        public string User { get; set; } = "";
    }
}