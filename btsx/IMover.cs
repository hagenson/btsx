namespace Btsx
{
    /// <summary>
    /// Specifies the operations that movers for different data type must implement.
    /// </summary>
    /// <remarks>
    /// A mover implementation provides the logic to migrate a set of data from one service to another.
    /// This interface abstract that logic so that movers for different types of data can be supported by a
    /// common service platform.
    /// </remarks>
    public interface IMover
    {
        /// <summary>
        /// Occurs when the migration status is updated during execution.
        /// </summary>
        event StatusEvent? StatusUpdate;

        /// <summary>
        /// Gets or sets a value indicating whether progress updates should be reported during migration.
        /// </summary>
        public bool ProgressUpdates { get; set; }

        /// <summary>
        /// Gets the statistics collected during the migration operation.
        /// </summary>
        public MigrationStats? Statistics { get; }

        /// <summary>
        /// Executes the migration operation.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Tests the authentication credentials for validity.
        /// </summary>
        /// <param name="creds">The credentials to test.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns<c>true</c> if authentication succeeds; otherwise, <c>false</c>.</returns>
        Task<bool> TestAuthenticationAsync(Creds creds, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Defines a generic interface for migration operations with typed credentials and options.
    /// </summary>
    /// <typeparam name="TCredentials">The type of credentials used for source and destination services.</typeparam>
    /// <typeparam name="TOptions">The type of options that configure the migration behavior.</typeparam>
    public interface IMover<TCredentials, TOptions> : IMover where TCredentials : class where TOptions : class
    {
        /// <summary>
        /// Gets or sets the credentials for the destination service.
        /// </summary>
        TCredentials? DestinationCredentials { get; set; }

        /// <summary>
        /// Gets or sets the options that configure the migration operation.
        /// </summary>
        TOptions? Options { get; set; }

        /// <summary>
        /// Gets or sets the credentials for the source service.
        /// </summary>
        TCredentials? SourceCredentials { get; set; }
    }
}