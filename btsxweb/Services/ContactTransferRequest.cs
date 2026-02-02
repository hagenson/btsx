namespace BtsxWeb.Services
{
    /// <summary>
    /// Encapsulates the parameters required to create a contact transfer job.
    /// </summary>
    public class ContactTransferRequest
    {
        /// <summary>
        /// OAuth token for destination account.
        /// </summary>
        public string? DestOAuthToken { get; set; }

        /// <summary>
        /// Password for Destination account.
        /// </summary>
        public string DestPassword { get; set; } = "";

        /// <summary>
        /// Hostname of destination server.
        /// </summary>
        public string DestServer { get; set; } = "";

        /// <summary>
        /// Type of destination service (e.g., "nextcloud").
        /// </summary>
        public string DestServiceType { get; set; } = "";

        /// <summary>
        /// True to use the DestOAuth token to access the destination account, false to use DestPassword.
        /// </summary>
        public bool DestUseOAuth { get; set; }

        /// <summary>
        /// Destination account username.
        /// </summary>
        public string DestUser { get; set; } = "";

        /// <summary>
        /// True to include progress in status updates.
        /// </summary>
        public bool ProgressUpdates { get; set; }

        /// <summary>
        /// True to overwrite existing contacts if they are found.
        /// </summary>
        public bool ReplaceExisting { get; set; }

        /// <summary>
        /// Source account OAuth token.
        /// </summary>
        public string? SourceOAuthToken { get; set; }

        /// <summary>
        /// Source account password.
        /// </summary>
        public string SourcePassword { get; set; } = "";

        /// <summary>
        /// Source server host name.
        /// </summary>
        public string SourceServer { get; set; } = "";

        /// <summary>
        /// Type of source service (e.g., "google").
        /// </summary>
        public string SourceServiceType { get; set; } = "";

        /// <summary>
        /// True to use the SourceOAuthToken to access the source server, false to use SourcePassword.
        /// </summary>
        public bool SourceUseOAuth { get; set; }

        /// <summary>
        /// Source account user name.
        /// </summary>
        public string SourceUser { get; set; } = "";
    }
}
