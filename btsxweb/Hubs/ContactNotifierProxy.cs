using BtsxWeb.Models;
using Microsoft.AspNetCore.SignalR;

namespace BtsxWeb.Hubs
{
    /// <summary>
    /// Provides an <see cref="IContactStatusNotifier"/> proxy that dispatches calls through the <see cref="ContactHub"/>.
    /// </summary>
    public class ContactNotifierProxy : IContactStatusNotifier
    {
        /// <summary>
        /// Initialises the proxy.
        /// </summary>
        public ContactNotifierProxy(IHubContext<ContactHub> context)
        {
            this.context = context;
        }

        /// <inheritdoc/>
        public Task NotifyStatusAsync(ContactTransferJobModel job, CancellationToken cancellationToken)
        {
            return context.Clients.Group(job.JobId).
                SendAsync("StatusUpdate", job, cancellationToken);
        }

        private readonly IHubContext<ContactHub> context;
    }
}
