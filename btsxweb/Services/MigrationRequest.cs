namespace BtsxWeb.Services
{
    /// <summary>
    /// Encapsulates the parameters required t create mail migration job.
    /// </summary>
    public class MigrationRequest
    {
        /// <summary>
        /// True to delete source message after it is transferred.
        /// </summary>
        public bool DeleteSource { get; set; }

        /// <summary>
        /// OAuth token for destination account.
        /// </summary>
        public string? DestOAuthToken { get; set; }

        /// <summary>
        /// Password for Destination account.
        /// </summary>
        public string DestPassword { get; set; } = "";

        /// <summary>
        /// HOstname of destination server.
        /// </summary>
        public string DestServer { get; set; } = "";

        /// <summary>
        /// True to use the DestOAuth token to access the destination account, false to use DestPassword.
        /// </summary>
        public bool DestUseOAuth { get; set; }

        /// <summary>
        /// Destination account username.
        /// </summary>
        public string DestUser { get; set; } = "";

        /// <summary>
        /// True to create folders structure only and don't transfer messages.
        /// </summary>
        public bool FoldersOnly { get; set; }

        /// <summary>
        /// True to include progress in status updates.
        /// </summary>
        public bool ProgressUpdates { get; set; }

        /// <summary>
        /// True to overwrite existing messages if they are found.
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
        /// True to use the SourceOAuthToken to access the source server, false to use SourcePasword.
        /// </summary>
        public bool SourceUseOAuth { get; set; }

        /// <summary>
        /// Source account user name.
        /// </summary>
        public string SourceUser { get; set; } = "";
    }
}