using BtsxWeb.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Abstracts persistent storage for migration jobs.
    /// </summary>
    public class JobPersistenceService
    {
        /// <summary>
        /// Initialises the service.
        /// </summary>
        public JobPersistenceService(
            ILogger<JobPersistenceService> logger,
            IOptions<PersistenceSettings> persistenceSettings,
            EncryptionService encryptionService,
            IWebHostEnvironment env)
        {
            this.logger = logger;
            this.encryptionService = encryptionService;

            if (persistenceSettings.Value.StorageDirectory == null)
                throw new InvalidOperationException("StorageDirectory must be specified.");
            jobStoragePath = persistenceSettings.Value.StorageDirectory;

            if (!Directory.Exists(jobStoragePath))
            {
                Directory.CreateDirectory(jobStoragePath);
                logger.LogInformation("Created job storage directory at {Path}", jobStoragePath);
            }
        }

        /// <summary>
        /// Deletes expired jobs.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        public async Task CleanupOldJobsAsync()
        {
            try
            {
                var allJobs = await LoadAllJobsAsync();
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                var oldJobs = allJobs.Where(j => j.StartTime < sevenDaysAgo).ToList();

                foreach (var job in oldJobs)
                {
                    DeleteJob(job.JobId);
                }

                if (oldJobs.Count > 0)
                {
                    logger.LogInformation("Cleaned up {Count} old jobs", oldJobs.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to cleanup old jobs");
            }
        }

        /// <summary>
        /// Deletes a persisted job.
        /// </summary>
        /// <param name="jobId">ID of the job to delete.</param>
        public void DeleteJob(string jobId)
        {
            try
            {
                var filePath = GetJobFilePath(jobId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    logger.LogInformation("Deleted job {JobId} from disk", jobId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete job {JobId} from disk", jobId);
            }
        }

        /// <summary>
        /// Gets all persisted migration jobs that have not yet completed.
        /// </summary>
        /// <returns>List of incomplete jobs.</returns>
        public async Task<List<MigrationJob>> GetIncompleteJobsAsync()
        {
            var allJobs = await LoadAllJobsAsync();
            return allJobs.Where(j => !j.IsCompleted).ToList();
        }

        /// <summary>
        /// Gets all persisted migration jobs.
        /// </summary>
        /// <returns>List of all available persisted jobs.</returns>
        public async Task<List<MigrationJob>> LoadAllJobsAsync()
        {
            var jobs = new List<MigrationJob>();

            try
            {
                var files = Directory.GetFiles(jobStoragePath, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var persistedJob = JsonSerializer.Deserialize<PersistedJobModel>(json);
                        if (persistedJob != null)
                        {
                            var decryptedRequest = new MigrationRequest
                            {
                                SourceServer = persistedJob.Request.SourceServer,
                                SourceUser = persistedJob.Request.SourceUser,
                                SourcePassword = encryptionService.Decrypt(persistedJob.Request.SourcePassword) ?? "",
                                SourceOAuthToken = encryptionService.Decrypt(persistedJob.Request.SourceOAuthToken),
                                SourceUseOAuth = persistedJob.Request.SourceUseOAuth,
                                DestServer = persistedJob.Request.DestServer,
                                DestUser = persistedJob.Request.DestUser,
                                DestPassword = encryptionService.Decrypt(persistedJob.Request.DestPassword) ?? "",
                                DestOAuthToken = encryptionService.Decrypt(persistedJob.Request.DestOAuthToken),
                                DestUseOAuth = persistedJob.Request.DestUseOAuth,
                                DeleteSource = persistedJob.Request.DeleteSource,
                                FoldersOnly = persistedJob.Request.FoldersOnly,
                                ProgressUpdates = persistedJob.Request.ProgressUpdates,
                                ReplaceExisting = persistedJob.Request.ReplaceExisting
                            };

                            var job = new MigrationJob
                            {
                                JobId = persistedJob.JobId,
                                Request = decryptedRequest,
                                Status = persistedJob.Status,
                                Progress = persistedJob.Progress,
                                StatusType = persistedJob.StatusType,
                                Statistics = persistedJob.Statistics,
                                StartTime = persistedJob.StartTime,
                                EndTime = persistedJob.EndTime,
                                IsCompleted = persistedJob.IsCompleted,
                                CancellationTokenSource = new CancellationTokenSource()
                            };
                            jobs.Add(job);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to load job from file {File}", file);
                    }
                }
                logger.LogInformation("Loaded {Count} jobs from disk", jobs.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load jobs from disk");
            }

            return jobs;
        }

        /// <summary>
        /// Loads and re-hydrates a persisted migration job.
        /// </summary>
        /// <param name="jobId">ID of the job to load.</param>
        /// <returns>Migration job.</returns>
        public async Task<MigrationJob?> LoadJobAsync(string jobId)
        {
            try
            {
                var filePath = GetJobFilePath(jobId);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var persistedJob = JsonSerializer.Deserialize<PersistedJobModel>(json);
                if (persistedJob == null)
                {
                    return null;
                }

                var decryptedRequest = new MigrationRequest
                {
                    SourceServer = persistedJob.Request.SourceServer,
                    SourceUser = persistedJob.Request.SourceUser,
                    SourcePassword = encryptionService.Decrypt(persistedJob.Request.SourcePassword) ?? "",
                    SourceOAuthToken = encryptionService.Decrypt(persistedJob.Request.SourceOAuthToken),
                    SourceUseOAuth = persistedJob.Request.SourceUseOAuth,
                    DestServer = persistedJob.Request.DestServer,
                    DestUser = persistedJob.Request.DestUser,
                    DestPassword = encryptionService.Decrypt(persistedJob.Request.DestPassword) ?? "",
                    DestOAuthToken = encryptionService.Decrypt(persistedJob.Request.DestOAuthToken),
                    DestUseOAuth = persistedJob.Request.DestUseOAuth,
                    DeleteSource = persistedJob.Request.DeleteSource,
                    FoldersOnly = persistedJob.Request.FoldersOnly,
                    ProgressUpdates = persistedJob.Request.ProgressUpdates,
                    ReplaceExisting = persistedJob.Request.ReplaceExisting
                };

                var job = new MigrationJob
                {
                    JobId = persistedJob.JobId,
                    Request = decryptedRequest,
                    Status = persistedJob.Status,
                    Progress = persistedJob.Progress,
                    StatusType = persistedJob.StatusType,
                    Statistics = persistedJob.Statistics,
                    StartTime = persistedJob.StartTime,
                    EndTime = persistedJob.EndTime,
                    IsCompleted = persistedJob.IsCompleted,
                    CancellationTokenSource = new CancellationTokenSource()
                };

                logger.LogInformation("Loaded job {JobId} from disk", jobId);
                return job;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load job {JobId} from disk", jobId);
                return null;
            }
        }

        /// <summary>
        /// Persists a migration job.
        /// </summary>
        /// <param name="job">Job to persist.</param>
        /// <returns>Awaitable task.</returns>
        public async Task SaveJobAsync(MigrationJob job)
        {
            try
            {
                var filePath = GetJobFilePath(job.JobId);
                var requestToSave = job.Request;

                if (job.IsCompleted)
                {
                    requestToSave = new MigrationRequest
                    {
                        SourceServer = job.Request.SourceServer,
                        SourceUser = job.Request.SourceUser,
                        SourcePassword = "",
                        SourceOAuthToken = null,
                        SourceUseOAuth = job.Request.SourceUseOAuth,
                        DestServer = job.Request.DestServer,
                        DestUser = job.Request.DestUser,
                        DestPassword = "",
                        DestOAuthToken = null,
                        DestUseOAuth = job.Request.DestUseOAuth,
                        DeleteSource = job.Request.DeleteSource,
                        FoldersOnly = job.Request.FoldersOnly,
                        ProgressUpdates = job.Request.ProgressUpdates,
                        ReplaceExisting = job.Request.ReplaceExisting
                    };
                }
                else
                {
                    requestToSave = new MigrationRequest
                    {
                        SourceServer = job.Request.SourceServer,
                        SourceUser = job.Request.SourceUser,
                        SourcePassword = encryptionService.Encrypt(job.Request.SourcePassword) ?? "",
                        SourceOAuthToken = encryptionService.Encrypt(job.Request.SourceOAuthToken),
                        SourceUseOAuth = job.Request.SourceUseOAuth,
                        DestServer = job.Request.DestServer,
                        DestUser = job.Request.DestUser,
                        DestPassword = encryptionService.Encrypt(job.Request.DestPassword) ?? "",
                        DestOAuthToken = encryptionService.Encrypt(job.Request.DestOAuthToken),
                        DestUseOAuth = job.Request.DestUseOAuth,
                        DeleteSource = job.Request.DeleteSource,
                        FoldersOnly = job.Request.FoldersOnly,
                        ProgressUpdates = job.Request.ProgressUpdates,
                        ReplaceExisting = job.Request.ReplaceExisting
                    };
                }

                var persistedJob = new PersistedJobModel
                {
                    JobId = job.JobId,
                    Request = requestToSave,
                    Status = job.Status,
                    Progress = job.Progress,
                    StatusType = job.StatusType,
                    Statistics = job.Statistics,
                    StartTime = job.StartTime,
                    EndTime = job.EndTime,
                    IsCompleted = job.IsCompleted
                };
                var json = JsonSerializer.Serialize(persistedJob, jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
                logger.LogInformation("Saved job {JobId} to disk", job.JobId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save job {JobId} to disk", job.JobId);
            }
        }

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly EncryptionService encryptionService;
        private readonly string jobStoragePath;
        private readonly ILogger<JobPersistenceService> logger;
        private string GetJobFilePath(string jobId)
        {
            return Path.Combine(jobStoragePath, $"{jobId}.json");
        }
    }
}