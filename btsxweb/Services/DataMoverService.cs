using AutoMapper;
using Btsx;
using BtsxWeb.Models;
using System.Collections.Concurrent;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Implements the logic for moving mail messages from one account to another.
    /// </summary>
    public class DataMoverService : IHostedService, IDisposable
    {
        /// <summary>
        /// Initialises the job.
        /// </summary>
        public DataMoverService(
            IServiceScopeFactory scopeFactory,
            IMapper mapper,
            ILogger<DataMoverService> logger,
            IPersistenceService persistenceService,
            IMoverFactory moverFactory)
        {
            this.scopeFactory = scopeFactory;
            this.mapper = mapper;
            this.logger = logger;
            this.persistenceService = persistenceService;
            this.moverFactory = moverFactory;
        }

        /// <summary>
        /// Cancels a running job.
        /// </summary>
        /// <param name="jobId">ID of job to cancel.</param>
        /// <returns>True if the job could be cancelled.</returns>
        public async Task<bool> CancelMigrationAsync(string jobId)
        {
            if (jobs.TryGetValue(jobId, out var running))
            {
                running.Job.IsCompleted = true;
                running.Job.Status = "Job cancelled.";
                await persistenceService.ClearProtectedPropertiesAsync(running.Job, stoppingCts!.Token);
                running.CancellationTokenSource.Cancel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete a running job.
        /// </summary>
        /// <param name="jobId">ID of the job to delete.</param>
        /// <returns>Awaitable task.</returns>
        public async Task DeleteJob(string jobId)
        {
            var job = await GetJob(jobId);
            if (job == null)
            {
                throw new InvalidOperationException($"Job {jobId} not found");
            }

            if (!job.IsCompleted)
            {
                throw new InvalidOperationException("Cannot delete a job that is not completed");
            }

            jobs.TryRemove(jobId, out _);
            await persistenceService.DeleteJobAsync(jobId, stoppingCts!.Token);
        }

        /// <summary>
        /// Cleans up the service resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Gets a migration job.
        /// </summary>
        /// <param name="jobId">ID of the job to get</param>
        /// <returns>Migration job if found.</returns>
        public async Task<MigrationJob?> GetJob(string jobId)
        {
            if (jobs.TryGetValue(jobId, out var running))
            {
                return (MigrationJob)running.Job;
            }

            var result = await persistenceService.LoadJobAsync(jobId, stoppingCts!.Token);
            return (MigrationJob?)result;
        }

        /// <summary>
        /// Starts the hosted service.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await RestoreIncompleteJobsAsync();
            await persistenceService.CleanupOldJobsAsync(cancellationToken);

            executingTask = ExecuteAsync(stoppingCts.Token);
        }

        /// <summary>
        /// Starts a single migration job running.
        /// </summary>
        /// <param name="request">Parameters to create the job from.</param>
        /// <returns>ID of the newly created migration job.</returns>
        public string StartMigration(MigrationRequest request)
        {
            var jobId = Guid.NewGuid().ToString("N");
            var job = new MigrationJob
            {
                Id = jobId,
                Request = request,
                StartTime = DateTime.Now,
                Status = "Starting",
                StatusType = "Info",
            };
            var running = new RunningJob(job);
            jobs[jobId] = running;

            Task.Run(async () =>
            {
                try
                {
                    await persistenceService.SaveJobAsync(job, stoppingCts!.Token);
                    await RunMigrationAsync(running);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error starting migration job {JobId}", jobId);
                }
            });

            return jobId;
        }

        /// <summary>
        /// Stops the hosted service.
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (executingTask == null)
            {
                return;
            }

            try
            {
                stoppingCts?.Cancel();
            }
            finally
            {
                await Task.WhenAny(executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private readonly ConcurrentDictionary<string, RunningJob> jobs = new();

        private readonly ILogger<DataMoverService> logger;

        private readonly IMapper mapper;

        private readonly IPersistenceService persistenceService;
        private readonly IMoverFactory moverFactory;
        private readonly IServiceScopeFactory scopeFactory;

        private Task? executingTask;

        private CancellationTokenSource? stoppingCts;

        ~DataMoverService()
        {
            Dispose(false);
        }

        private void Dispose(bool isDisposing)
        {
            if (stoppingCts != null)
            {
                stoppingCts.Cancel();
                stoppingCts.Dispose();
                stoppingCts = null;
            }

            if (isDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await persistenceService.CleanupOldJobsAsync(stoppingToken);
                }
            }
        }

        private async Task RestoreIncompleteJobsAsync()
        {
            try
            {
                var incompleteJobs = await persistenceService.GetIncompleteJobsAsync(stoppingCts!.Token);

                foreach (MigrationJob job in incompleteJobs)
                {
                    logger.LogInformation("Restoring incomplete job {JobId}", job.Id);
                    var running = new RunningJob(job);
                    job.Request.Restarting();
                    job.Status = "Restarting";
                    job.Progress = 0;

                    jobs[job.Id] = running;

                    _ = Task.Run(async () => await RunMigrationAsync(running));
                }

                if (incompleteJobs.Count > 0)
                {
                    logger.LogInformation("Restored {Count} incomplete jobs", incompleteJobs.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to restore incomplete jobs");
            }
        }

        private async Task RevokeOAuthTokensAsync(
            IServiceProvider serviceProvider,
            IJob job,
            IStatusNotifier notifier)
        {
            foreach (var credential in new[] { job.Request?.SourceCredentials, job.Request?.DestinationCredentials })
            {
                if (credential?.UseOAuth == true && !string.IsNullOrEmpty(credential.OAuthToken))
                {
                    var oauthSvc = serviceProvider.GetRequiredKeyedService<IOAuthService>(credential.Implementer);
                    try
                    {
                        await oauthSvc.RevokeTokenAsync(credential.OAuthToken, stoppingCts!.Token);
                        job.Status = $"Successfully revoked OAuth token for {credential.Server}.";
                        job.StatusType = "Info";
                        await notifier.NotifyStatusAsync(mapper.Map<MigrationJobModel>(job), stoppingCts!.Token);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to revoke OAuth token for {Server} in job {JobId}",
                            credential.Server, job.Id);
                        job.Status = $"Warning: Failed to revoke OAuth token for {credential.Server}.";
                        job.StatusType = "Warning";
                        await notifier.NotifyStatusAsync(mapper.Map<MigrationJobModel>(job), stoppingCts!.Token);
                    }
                }
            }
        }

        private async Task RunMigrationAsync(RunningJob running)
        {
            if (stoppingCts == null)
                throw new InvalidOperationException($"{nameof(stoppingCts)} has not been initialised.");
            using (var scope = scopeFactory.CreateScope())
            {
                var notifier = scope.ServiceProvider.GetRequiredService<IStatusNotifier>();
                try
                {
                    var mover = moverFactory.CreateMover(running.Job.Request);

                    mover.StatusUpdate += async (sender, e) =>
                    {
                        running.Job.Status = e.Status ?? "";
                        running.Job.Progress = e.Percentage;
                        running.Job.StatusType = e.Type.ToString();
                        await notifier.NotifyStatusAsync(mapper.Map<MigrationJobModel>(running.Job), stoppingCts.Token);
                    };

                    await mover.ExecuteAsync(running.CancellationTokenSource.Token);

                    if (running.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await RevokeOAuthTokensAsync(scope.ServiceProvider, running.Job, notifier);

                        running.Job.Status = "Cancelled by user";
                        running.Job.StatusType = "Warning";
                        running.Job.EndTime = DateTime.UtcNow;
                        running.Job.IsCompleted = true;
                        await notifier.NotifyStatusAsync(mapper.Map<MigrationJobModel>(running.Job), stoppingCts.Token);
                    }
                    else
                    {
                        await RevokeOAuthTokensAsync(scope.ServiceProvider, running.Job, notifier);

                        running.Job.Status = "Completed";
                        running.Job.Progress = 100;
                        running.Job.Statistics = mover.Statistics;
                        running.Job.EndTime = DateTime.UtcNow;
                        running.Job.IsCompleted = true;
                        await persistenceService.ClearProtectedPropertiesAsync(running.Job, stoppingCts!.Token);
                        await notifier.NotifyStatusAsync(mapper.Map<MigrationJobModel>(running.Job), stoppingCts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellations - service may have terminated, and user cancelled jobs are already persisted
                }
                catch (Exception ex)
                {
                    await RevokeOAuthTokensAsync(scope.ServiceProvider, running.Job, notifier);

                    running.Job.Status = $"Error: {ex.Message}";
                    running.Job.StatusType = "Error";
                    running.Job.EndTime = DateTime.UtcNow;
                    running.Job.IsCompleted = true;
                    logger.LogError(ex, "Error during migration for job {JobId}", running.Job.Id);
                    await persistenceService.SaveJobAsync(running.Job, stoppingCts!.Token);
                    await notifier.NotifyStatusAsync(mapper.Map<MigrationJobModel>(running.Job), stoppingCts.Token);
                }
            }
        }
    }
}