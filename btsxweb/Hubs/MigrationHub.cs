using BtsxWeb.Models;
using BtsxWeb.Services;
using Microsoft.AspNetCore.SignalR;

namespace BtsxWeb.Hubs
{
    /// <summary>
    /// Provides the SignalR hub for the application.
    /// </summary>
    public class MigrationHub : Hub
    {
        /// <summary>
        /// Initialises the hub.
        /// </summary>
        public MigrationHub(MailMoverService mailMoverService, Mapper mapper)
        {
            this.mailMoverService = mailMoverService;
            this.mapper = mapper;
        }

        /// <summary>
        /// Cancels a running migration job.
        /// </summary>
        /// <param name="jobId">ID of the job to cancel.</param>
        /// <returns>Awaitable Task.</returns>
        public async Task CancelMigration(string jobId)
        {
            var cancelled = await mailMoverService.CancelMigrationAsync(jobId);
            if (cancelled)
            {
                await Clients.Group(jobId).SendAsync("JobCancelled");
            }
        }

        /// <summary>
        /// Deletes a migration job.
        /// </summary>
        /// <param name="jobId">ID of the job to delete.</param>
        /// <returns>Awaitable Task.</returns>
        public async Task DeleteJob(string jobId)
        {
            await mailMoverService.DeleteJob(jobId);
        }

        /// <summary>
        /// Gets a migration job.
        /// </summary>
        /// <param name="jobId">ID of the job to get.</param>
        /// <returns>Migration job model.</returns>
        public async Task<MigrationJobModel?> GetJobInfo(string jobId)
        {
            var job = await mailMoverService.GetJob(jobId);
            if (job == null)
            {
                return null;
            }

            return mapper.Map(job);
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
        /// Starts a migration job.
        /// </summary>
        /// <param name="request">Parameters for the job.</param>
        /// <returns>Awaitable Task.</returns>
        public async Task StartMigration(MigrationRequest request)
        {
            var jobId = mailMoverService.StartMigration(request);
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId, Context.ConnectionAborted);
            await Clients.Caller.SendAsync("MigrationStarted", jobId);
        }

        private readonly MailMoverService mailMoverService;
        private readonly Mapper mapper;
    }
}