using BtsxWeb.Models;
using Microsoft.AspNetCore.SignalR;

namespace BtsxWeb.Hubs
{
    /// <summary>
    /// Provides an <see cref="IStatusNotifier"/> proxy that dispatches calls through the <see cref="MigrationHub"/>.
    /// </summary>
    public class NotifierProxy : IStatusNotifier
    {
        /// <summary>
        /// Initialises the proxy.
        /// </summary>
        public NotifierProxy(IHubContext<MigrationHub> context)
        {
            this.context = context;
        }

        /// <inheritdoc/>
        public Task NotifyStatusAsync(MigrationJobModel job, CancellationToken cancellationToken)
        {
            return context.Clients.Group(job.JobId).
                SendAsync("StatusUpdate", job, cancellationToken);
        }

        private readonly IHubContext<MigrationHub> context;
    }
}