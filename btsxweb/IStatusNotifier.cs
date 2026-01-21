using BtsxWeb.Models;

namespace BtsxWeb
{
    /// <summary>
    /// Defines operations to send notifications to web clients.
    /// </summary>
    public interface IStatusNotifier
    {
        /// <summary>
        /// Sends a notification to clients that have registered for status updates for a migration job.
        /// </summary>
        /// <param name="job">Migration job status information.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Awaitable Task.</returns>
        public Task NotifyStatusAsync(MigrationJobModel job, CancellationToken cancellationToken);
    }
}