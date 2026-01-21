using Btsx;
using BtsxWeb.Models;
using System.Collections.Concurrent;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Implements the logic for moving mail messages from one account to another.
    /// </summary>
    public class MailMoverService : IHostedService, IDisposable
    {
        /// <summary>
        /// Initialises the job.
        /// </summary>
        public MailMoverService(
            IServiceScopeFactory scopeFactory,
            Mapper mapper,
            ILogger<MailMoverService> logger,
            JobPersistenceService persistenceService,
            GoogleOAuthService tokenRevocationService)
        {
            this.scopeFactory = scopeFactory;
            this.mapper = mapper;
            this.logger = logger;
            this.persistenceService = persistenceService;
            this.tokenRevocationService = tokenRevocationService;
        }

        /// <summary>
        /// Cancels a running job.
        /// </summary>
        /// <param name="jobId">ID of job to cancel.</param>
        /// <returns>True if the job could be cancelled.</returns>
        public bool CancelMigration(string jobId)
        {
            if (jobs.TryGetValue(jobId, out var job))
            {
                job.CancellationTokenSource.Cancel();
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
            persistenceService.DeleteJob(jobId);
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
            if (jobs.TryGetValue(jobId, out var job))
            {
                return job;
            }

            return await persistenceService.LoadJobAsync(jobId);
        }


        /// <summary>
        /// Starts the hosted service.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await RestoreIncompleteJobsAsync();
            await persistenceService.CleanupOldJobsAsync();

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
                JobId = jobId,
                Request = request,
                Status = "Starting",
                Progress = 0,
                StartTime = DateTime.UtcNow,
                IsCompleted = false,
                CancellationTokenSource = new CancellationTokenSource()
            };

            jobs[jobId] = job;

            Task.Run(async () =>
            {
                await persistenceService.SaveJobAsync(job);
                await RunMigrationAsync(job);
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

        private readonly ConcurrentDictionary<string, MigrationJob> jobs = new();

        private readonly ILogger<MailMoverService> logger;

        private readonly Mapper mapper;

        private readonly JobPersistenceService persistenceService;

        private readonly IServiceScopeFactory scopeFactory;

        private readonly GoogleOAuthService tokenRevocationService;

        private Task? executingTask;

        private CancellationTokenSource? stoppingCts;

        ~MailMoverService()
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
                    await persistenceService.CleanupOldJobsAsync();
                }
            }
        }

        private async Task RestoreIncompleteJobsAsync()
        {
            try
            {
                var incompleteJobs = await persistenceService.GetIncompleteJobsAsync();

                foreach (var job in incompleteJobs)
                {
                    logger.LogInformation("Restoring incomplete job {JobId}", job.JobId);

                    job.Request.ReplaceExisting = true;
                    job.CancellationTokenSource = new CancellationTokenSource();
                    job.Status = "Restarting";
                    job.Progress = 0;

                    jobs[job.JobId] = job;

                    _ = Task.Run(async () => await RunMigrationAsync(job));
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

        private async Task RevokeOAuthTokensAsync(MigrationJob job, IStatusNotifier notifier)
        {
            try
            {
                var (sourceRevoked, destRevoked) = await tokenRevocationService.RevokeJobTokensAsync(job);

                var revokedTokens = new List<string>();
                if (sourceRevoked && job.Request.SourceUseOAuth)
                {
                    revokedTokens.Add("source");
                }
                if (destRevoked && job.Request.DestUseOAuth)
                {
                    revokedTokens.Add("destination");
                }

                if (!sourceRevoked || !destRevoked)
                {
                    var warningMessages = new List<string>();
                    if (!sourceRevoked && job.Request.SourceUseOAuth)
                    {
                        warningMessages.Add("source");
                    }
                    if (!destRevoked && job.Request.DestUseOAuth)
                    {
                        warningMessages.Add("destination");
                    }

                    if (warningMessages.Count > 0)
                    {
                        var tempStatus = job.Status;
                        var tempStatusType = job.StatusType;

                        job.Status = $"Warning: Failed to revoke {string.Join(" and ", warningMessages)} OAuth token(s)";
                        job.StatusType = "Warning";
                        await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts!.Token);

                        job.Status = tempStatus;
                        job.StatusType = tempStatusType;

                        logger.LogWarning("Failed to revoke OAuth tokens for job {JobId}: {Tokens}",
                            job.JobId, string.Join(" and ", warningMessages));
                    }
                }

                if (revokedTokens.Count > 0)
                {
                    var tempStatus = job.Status;
                    var tempStatusType = job.StatusType;

                    job.Status = $"Successfully revoked {string.Join(" and ", revokedTokens)} OAuth token(s)";
                    job.StatusType = "Info";
                    await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts!.Token);

                    job.Status = tempStatus;
                    job.StatusType = tempStatusType;

                    logger.LogInformation("Successfully revoked OAuth tokens for job {JobId}: {Tokens}",
                        job.JobId, string.Join(" and ", revokedTokens));
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Exception occurred while revoking OAuth tokens for job {JobId}", job.JobId);

                var tempStatus = job.Status;
                var tempStatusType = job.StatusType;

                job.Status = "Warning: Failed to revoke OAuth token(s) due to an error";
                job.StatusType = "Warning";
                await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts!.Token);

                job.Status = tempStatus;
                job.StatusType = tempStatusType;
            }
        }

        private async Task RunMigrationAsync(MigrationJob job)
        {
            if (stoppingCts == null)
                throw new InvalidOperationException($"{nameof(stoppingCts)} has not been initialised.");

            using (var scope = scopeFactory.CreateScope())
            {
                var notifier = scope.ServiceProvider.GetRequiredService<IStatusNotifier>();
                try
                {
                    var mover = new MailMover
                    {
                        SourceCredentials = new Creds
                        {
                            Server = job.Request.SourceServer,
                            User = job.Request.SourceUser,
                            Password = job.Request.SourcePassword,
                            OAuthToken = job.Request.SourceOAuthToken,
                            UseOAuth = job.Request.SourceUseOAuth
                        },
                        DestCredentials = new Creds
                        {
                            Server = job.Request.DestServer,
                            User = job.Request.DestUser,
                            Password = job.Request.DestPassword,
                            OAuthToken = job.Request.DestOAuthToken,
                            UseOAuth = job.Request.DestUseOAuth
                        },
                        DeleteSource = job.Request.DeleteSource,
                        FoldersOnly = job.Request.FoldersOnly,
                        ProgressUpdates = job.Request.ProgressUpdates,
                        ReplaceExisting = job.Request.ReplaceExisting
                    };

                    mover.StatusUpdate += async (sender, e) =>
                    {
                        job.Status = e.Status ?? "";
                        job.Progress = e.Percentage;
                        job.StatusType = e.Type.ToString();
                        await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);
                    };

                    await mover.ExecuteAsync(job.CancellationTokenSource.Token);

                    if (job.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await RevokeOAuthTokensAsync(job, notifier);

                        job.Status = "Cancelled by user";
                        job.StatusType = "Warning";
                        job.EndTime = DateTime.UtcNow;
                        job.IsCompleted = true;
                        await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);
                    }
                    else
                    {
                        await RevokeOAuthTokensAsync(job, notifier);

                        job.Status = "Completed";
                        job.Progress = 100;
                        job.Statistics = mover.Statistics;
                        job.EndTime = DateTime.UtcNow;
                        job.IsCompleted = true;
                        await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);
                    }
                }
                catch (Exception ex)
                {
                    await RevokeOAuthTokensAsync(job, notifier);

                    job.Status = $"Error: {ex.Message}";
                    job.StatusType = "Error";
                    job.EndTime = DateTime.UtcNow;
                    job.IsCompleted = true;
                    logger.LogError(ex, "Error during migration for job {JobId}", job.JobId);
                    await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);
                }
                finally
                {
                    await persistenceService.SaveJobAsync(job);
                }
            }
        }
    }
}