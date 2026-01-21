namespace BtsxWeb.Models
{
    /// <summary>
    /// Encapsulates connection details for Google OAuth token acquisition.
    /// </summary>
    public class GoogleOAuthSettings
    {
        /// <summary>
        /// The Google Client ID.
        /// </summary>
        public string ClientId { get; set; } = "";
        
        /// <summary>
        /// The Google Client Secret.
        /// </summary>
        public string ClientSecret { get; set; } = "";

        /// <summary>
        /// The Redirect URL to pass with the Google OAuth request.
        /// </summary>
        public string RedirectUri { get; set; } = "";
    }
}