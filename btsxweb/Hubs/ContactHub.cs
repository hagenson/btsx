using BtsxWeb.Models;
using BtsxWeb.Services;
using Microsoft.AspNetCore.SignalR;

namespace BtsxWeb.Hubs
{
    /// <summary>
    /// Provides the SignalR hub for contact transfer operations.
    /// </summary>
    public class ContactHub : Hub
    {
        /// <summary>
        /// Initialises the hub.
        /// </summary>
        public ContactHub(ContactMoverService contactMoverService, ContactMapper mapper)
        {
            this.contactMoverService = contactMoverService;
            this.mapper = mapper;
        }

        /// <summary>
        /// Cancels a running contact transfer job.
        /// </summary>
        /// <param name="jobId">ID of the job to cancel.</param>
        /// <returns>Awaitable Task.</returns>
        public async Task CancelContactTransfer(string jobId)
        {
            var cancelled = contactMoverService.CancelTransfer(jobId);
            if (cancelled)
            {
                await Clients.Group(jobId).SendAsync("JobCancelled");
            }
        }

        /// <summary>
        /// Deletes a contact transfer job.
        /// </summary>
        /// <param name="jobId">ID of the job to delete.</param>
        /// <returns>Awaitable Task.</returns>
        public async Task DeleteJob(string jobId)
        {
            await contactMoverService.DeleteJob(jobId);
        }

        /// <summary>
        /// Adds the current connection to the specified job group.
        /// </summary>
        /// <param name="jobId">The identifier of the job group to join. </param>
        /// <returns>Awaitable Task.</returns>
        public async Task JoinJobGroup(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId, Context.ConnectionAborted);
        }

        /// <summary>
        /// Starts a contact transfer job.
        /// </summary>
        /// <param name="request">Parameters for the job.</param>
        /// <returns>Awaitable Task.</returns>
        public async Task StartContactTransfer(ContactTransferRequest request)
        {
            var jobId = contactMoverService.StartTransfer(request);
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId, Context.ConnectionAborted);
            await Clients.Caller.SendAsync("TransferStarted", jobId);
        }

        private readonly ContactMoverService contactMoverService;
        private readonly ContactMapper mapper;
    }
}
