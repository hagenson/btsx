using Btsx;
using BtsxWeb.Models;
using System.Collections.Concurrent;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Implements the logic for moving contacts from one account to another.
    /// </summary>
    public class ContactMoverService : IHostedService, IDisposable
    {
        /// <summary>
        /// Initialises the job.
        /// </summary>
        public ContactMoverService(
            IServiceScopeFactory scopeFactory,
            ContactMapper mapper,
            ILogger<ContactMoverService> logger,
            ContactJobPersistenceService persistenceService,
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
        public bool CancelTransfer(string jobId)
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
        /// Gets a contact transfer job.
        /// </summary>
        /// <param name="jobId">ID of the job to get</param>
        /// <returns>Contact transfer job if found.</returns>
        public async Task<ContactTransferJob?> GetJob(string jobId)
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
        /// Starts a single contact transfer job running.
        /// </summary>
        /// <param name="request">Parameters to create the job from.</param>
        /// <returns>ID of the newly created contact transfer job.</returns>
        public string StartTransfer(ContactTransferRequest request)
        {
            var jobId = Guid.NewGuid().ToString("N");
            var job = new ContactTransferJob
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
                await RunTransferAsync(job);
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

        private readonly ConcurrentDictionary<string, ContactTransferJob> jobs = new();

        private readonly ILogger<ContactMoverService> logger;

        private readonly ContactMapper mapper;

        private readonly ContactJobPersistenceService persistenceService;

        private readonly IServiceScopeFactory scopeFactory;

        private readonly GoogleOAuthService tokenRevocationService;

        private Task? executingTask;

        private CancellationTokenSource? stoppingCts;

        ~ContactMoverService()
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

                    job.CancellationTokenSource = new CancellationTokenSource();
                    job.Status = "Restarting";
                    job.Progress = 0;

                    jobs[job.JobId] = job;

                    _ = Task.Run(async () => await RunTransferAsync(job));
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

        private async Task RevokeOAuthTokensAsync(ContactTransferJob job, IContactStatusNotifier notifier)
        {
            try
            {
                var sourceRevoked = true;
                var destRevoked = true;

                if (job.Request.SourceUseOAuth && !string.IsNullOrWhiteSpace(job.Request.SourceOAuthToken))
                {
                    logger.LogInformation("Revoking source OAuth token for job {JobId}", job.JobId);
                    sourceRevoked = await tokenRevocationService.RevokeTokenAsync(job.Request.SourceOAuthToken);
                }

                if (job.Request.DestUseOAuth && !string.IsNullOrWhiteSpace(job.Request.DestOAuthToken))
                {
                    logger.LogInformation("Revoking destination OAuth token for job {JobId}", job.JobId);
                    destRevoked = await tokenRevocationService.RevokeTokenAsync(job.Request.DestOAuthToken);
                }

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

        private async Task RunTransferAsync(ContactTransferJob job)
        {
            if (stoppingCts == null)
                throw new InvalidOperationException($"{nameof(stoppingCts)} has not been initialised.");

            using (var scope = scopeFactory.CreateScope())
            {
                var notifier = scope.ServiceProvider.GetRequiredService<IContactStatusNotifier>();
                try
                {
                    job.Status = "Starting contact transfer...";
                    job.StatusType = "Info";
                    await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);

                    List<ContactData> contacts;
                    
                    if (job.Request.SourceServiceType.Equals("google", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrWhiteSpace(job.Request.SourceOAuthToken))
                        {
                            throw new InvalidOperationException("Source OAuth token is required for Google contacts");
                        }

                        var googleService = new GoogleContactsService(job.Request.SourceOAuthToken);
                        
                        job.Status = "Fetching contacts from Google...";
                        await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);

                        contacts = await googleService.FetchContactsAsync(
                            (count, message) =>
                            {
                                job.Status = message;
                                _ = notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);
                            },
                            job.CancellationTokenSource.Token);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported source service type: {job.Request.SourceServiceType}");
                    }

                    if (job.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await RevokeOAuthTokensAsync(job, notifier);
                        job.Status = "Cancelled by user";
                        job.StatusType = "Warning";
                        job.EndTime = DateTime.UtcNow;
                        job.IsCompleted = true;
                        await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);
                        return;
                    }

                    job.Status = $"Fetched {contacts.Count} contacts";
                    await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);

                    if (job.Request.DestServiceType.Equals("nextcloud", StringComparison.OrdinalIgnoreCase))
                    {
                        var nextCloudService = new NextCloudContactsService(
                            job.Request.DestServer,
                            job.Request.DestUser,
                            job.Request.DestPassword);

                        job.Status = "Testing NextCloud connection...";
                        await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);

                        var connected = await nextCloudService.TestConnectionAsync(job.CancellationTokenSource.Token);
                        if (!connected)
                        {
                            throw new InvalidOperationException("Failed to connect to NextCloud server");
                        }

                        job.Status = "Uploading contacts to NextCloud...";
                        await notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);

                        var result = await nextCloudService.UploadContactsAsync(
                            contacts,
                            (current, total, message) =>
                            {
                                job.Status = message;
                                if (job.Request.ProgressUpdates && total > 0)
                                {
                                    job.Progress = (int)((decimal)current / total * 100);
                                }
                                _ = notifier.NotifyStatusAsync(mapper.Map(job), stoppingCts.Token);
                            },
                            job.CancellationTokenSource.Token);

                        var stats = new MigrationStats
                        {
                            TotalMessages = result.Total,
                            SuccessfulMessages = result.Successful,
                            FailedMessages = result.Failed,
                            SkippedMessages = 0
                        };

                        job.Statistics = stats;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported destination service type: {job.Request.DestServiceType}");
                    }

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
                    logger.LogError(ex, "Error during contact transfer for job {JobId}", job.JobId);
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
