using BtsxWeb.Models;

namespace BtsxWeb
{
    /// <summary>
    /// Defines operations to send notifications to web clients for contact transfer jobs.
    /// </summary>
    public interface IContactStatusNotifier
    {
        /// <summary>
        /// Sends a notification to clients that have registered for status updates for a contact transfer job.
        /// </summary>
        /// <param name="job">Contact transfer job status information.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Awaitable Task.</returns>
        public Task NotifyStatusAsync(ContactTransferJobModel job, CancellationToken cancellationToken);
    }
}
