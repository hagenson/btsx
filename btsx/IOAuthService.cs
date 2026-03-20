namespace Btsx
{
    public interface IOAuthService
    {
        /// <summary>
        /// Gets the URL to redirect to start the OAuth login.
        /// </summary>
        /// <param name="type">Type of migration being initiated.</param>
        /// <param name="direction">Whether the URL is for the source or destination account.</param>
        /// <param name="state">State data to pass with the URL.</param>
        /// <returns>URL that can be used to initiate the login.</returns>
        string GetAuthUrl(MigrationType type, MigrationDirection direction, string state);

        /// <summary>
        /// Exchanges the code received from the OAuth login for an access token and related information.
        /// </summary>
        /// <param name="code">Code received from the OAuth login.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Token response information.</returns>
        Task<TokenResponse> RequestTokenAsync(
            string code,
            CancellationToken cancellationToken);

        /// <summary>
        /// Revokes a previously issued OAuth token.
        /// </summary>
        /// <param name="token">Token to revoke.</param>
        /// <param name="cancellationToken"> Cancellation token.</param>
        /// <returns>Success or failure of operation.</returns>
        Task<bool> RevokeTokenAsync(string? token, CancellationToken cancellationToken);
    }

    public enum MigrationType
    {
        Mail,
        Contacts
    }

    public enum MigrationDirection
    {
        Source,
        Destination
    }
}