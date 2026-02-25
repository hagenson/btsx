using Btsx;

namespace BtsxWeb
{
    /// <summary>
    /// Defines operations to instantiate mover for different migration job types.
    /// </summary>
    public interface IMoverFactory
    {
        /// <summary>
        /// Creates a mover for the specified request, ready to call the <see cref="IMover.TestAuthenticationAsync(Creds, CancellationToken)"/>
        /// method.
        /// </summary>
        /// <param name="migrationType">Type of migration job being requested.</param>
        /// <returns>Mover than can support a test authentication call.</returns>
        /// <remarks>
        /// A mover will not need to be fully initialised to test authentication credentials.
        /// This operation provides a way to get a partially initialised mover that can be used to
        /// validate authentication credentials before calling <see cref="CreateMover(MigrationRequest)"/>
        /// to execute a migration job.
        /// </remarks>
        IMover CreateAuthenticator(MigrationType migrationType);

        /// <summary>
        /// Creates and initialises a mover for the specified request, ready to execute a migration job.
        /// </summary>
        /// <param name="request">The migration job being requested.</param>
        /// <returns>Initialised mover.</returns>
        IMover CreateMover(MigrationRequest request);
    }
}