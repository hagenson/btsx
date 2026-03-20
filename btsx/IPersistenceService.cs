namespace Btsx
{
    /// <summary>
    /// Defines the interface for persisting and managing job data.
    /// </summary>
    public interface IPersistenceService
    {
        /// <summary>
        /// Cleans up old jobs from the persistence store.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task CleanupOldJobsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Clears protected properties from the specified job and persists it.
        /// </summary>
        /// <param name="job">The job whose protected properties should be cleared.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ClearProtectedPropertiesAsync(IJob job, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the job with the specified identifier.
        /// </summary>
        /// <param name="jobId">The identifier of the job to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DeleteJobAsync(string jobId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all incomplete jobs from the persistence store.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of jobs with a null <see cref="IJob.EndTime"/>.</returns>
        Task<List<IJob>> GetIncompleteJobsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Loads all jobs from the persistence store.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of all jobs.</returns>
        Task<List<IJob>> LoadAllJobsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Loads the job with the specified identifier.
        /// </summary>
        /// <param name="jobId">The identifier of the job to load.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The job if found; otherwise, <c>null</c>.</returns>
        Task<IJob?> LoadJobAsync(string jobId, CancellationToken cancellationToken);

        /// <summary>
        /// Saves the specified job to the persistence store.
        /// </summary>
        /// <param name="job">The job to save.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SaveJobAsync(IJob job, CancellationToken cancellationToken);
    }
}