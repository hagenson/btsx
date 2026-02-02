using BtsxWeb.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Abstracts persistent storage for contact transfer jobs.
    /// </summary>
    public class ContactJobPersistenceService
    {
        /// <summary>
        /// Initialises the service.
        /// </summary>
        public ContactJobPersistenceService(
            ILogger<ContactJobPersistenceService> logger,
            IOptions<PersistenceSettings> persistenceSettings,
            EncryptionService encryptionService,
            IWebHostEnvironment env)
        {
            this.logger = logger;
            this.encryptionService = encryptionService;

            if (persistenceSettings.Value.StorageDirectory == null)
                throw new InvalidOperationException("StorageDirectory must be specified.");
            jobStoragePath = Path.Combine(persistenceSettings.Value.StorageDirectory, "contacts");

            if (!Directory.Exists(jobStoragePath))
            {
                Directory.CreateDirectory(jobStoragePath);
                logger.LogInformation("Created contact job storage directory at {Path}", jobStoragePath);
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
                    logger.LogInformation("Cleaned up {Count} old contact jobs", oldJobs.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to cleanup old contact jobs");
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
                    logger.LogInformation("Deleted contact job {JobId} from disk", jobId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete contact job {JobId} from disk", jobId);
            }
        }

        /// <summary>
        /// Gets all persisted contact transfer jobs that have not yet completed.
        /// </summary>
        /// <returns>List of incomplete jobs.</returns>
        public async Task<List<ContactTransferJob>> GetIncompleteJobsAsync()
        {
            var allJobs = await LoadAllJobsAsync();
            return allJobs.Where(j => !j.IsCompleted).ToList();
        }

        /// <summary>
        /// Gets all persisted contact transfer jobs.
        /// </summary>
        /// <returns>List of all available persisted jobs.</returns>
        public async Task<List<ContactTransferJob>> LoadAllJobsAsync()
        {
            var jobs = new List<ContactTransferJob>();

            try
            {
                var files = Directory.GetFiles(jobStoragePath, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var persistedJob = JsonSerializer.Deserialize<PersistedContactJobModel>(json);
                        if (persistedJob != null)
                        {
                            var decryptedRequest = new ContactTransferRequest
                            {
                                SourceServer = persistedJob.Request.SourceServer,
                                SourceUser = persistedJob.Request.SourceUser,
                                SourcePassword = encryptionService.Decrypt(persistedJob.Request.SourcePassword) ?? "",
                                SourceOAuthToken = encryptionService.Decrypt(persistedJob.Request.SourceOAuthToken),
                                SourceUseOAuth = persistedJob.Request.SourceUseOAuth,
                                SourceServiceType = persistedJob.Request.SourceServiceType,
                                DestServer = persistedJob.Request.DestServer,
                                DestUser = persistedJob.Request.DestUser,
                                DestPassword = encryptionService.Decrypt(persistedJob.Request.DestPassword) ?? "",
                                DestOAuthToken = encryptionService.Decrypt(persistedJob.Request.DestOAuthToken),
                                DestUseOAuth = persistedJob.Request.DestUseOAuth,
                                DestServiceType = persistedJob.Request.DestServiceType,
                                ProgressUpdates = persistedJob.Request.ProgressUpdates
                            };

                            var job = new ContactTransferJob
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
                        logger.LogError(ex, "Failed to load contact job from file {File}", file);
                    }
                }
                logger.LogInformation("Loaded {Count} contact jobs from disk", jobs.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load contact jobs from disk");
            }

            return jobs;
        }

        /// <summary>
        /// Loads and re-hydrates a persisted contact transfer job.
        /// </summary>
        /// <param name="jobId">ID of the job to load.</param>
        /// <returns>Contact transfer job.</returns>
        public async Task<ContactTransferJob?> LoadJobAsync(string jobId)
        {
            try
            {
                var filePath = GetJobFilePath(jobId);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var persistedJob = JsonSerializer.Deserialize<PersistedContactJobModel>(json);
                if (persistedJob == null)
                {
                    return null;
                }

                var decryptedRequest = new ContactTransferRequest
                {
                    SourceServer = persistedJob.Request.SourceServer,
                    SourceUser = persistedJob.Request.SourceUser,
                    SourcePassword = encryptionService.Decrypt(persistedJob.Request.SourcePassword) ?? "",
                    SourceOAuthToken = encryptionService.Decrypt(persistedJob.Request.SourceOAuthToken),
                    SourceUseOAuth = persistedJob.Request.SourceUseOAuth,
                    SourceServiceType = persistedJob.Request.SourceServiceType,
                    DestServer = persistedJob.Request.DestServer,
                    DestUser = persistedJob.Request.DestUser,
                    DestPassword = encryptionService.Decrypt(persistedJob.Request.DestPassword) ?? "",
                    DestOAuthToken = encryptionService.Decrypt(persistedJob.Request.DestOAuthToken),
                    DestUseOAuth = persistedJob.Request.DestUseOAuth,
                    DestServiceType = persistedJob.Request.DestServiceType,
                    ProgressUpdates = persistedJob.Request.ProgressUpdates
                };

                var job = new ContactTransferJob
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

                logger.LogInformation("Loaded contact job {JobId} from disk", jobId);
                return job;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load contact job {JobId} from disk", jobId);
                return null;
            }
        }

        /// <summary>
        /// Persists a contact transfer job.
        /// </summary>
        /// <param name="job">Job to persist.</param>
        /// <returns>Awaitable task.</returns>
        public async Task SaveJobAsync(ContactTransferJob job)
        {
            try
            {
                var filePath = GetJobFilePath(job.JobId);
                var requestToSave = job.Request;

                if (job.IsCompleted)
                {
                    requestToSave = new ContactTransferRequest
                    {
                        SourceServer = job.Request.SourceServer,
                        SourceUser = job.Request.SourceUser,
                        SourcePassword = "",
                        SourceOAuthToken = null,
                        SourceUseOAuth = job.Request.SourceUseOAuth,
                        SourceServiceType = job.Request.SourceServiceType,
                        DestServer = job.Request.DestServer,
                        DestUser = job.Request.DestUser,
                        DestPassword = "",
                        DestOAuthToken = null,
                        DestUseOAuth = job.Request.DestUseOAuth,
                        DestServiceType = job.Request.DestServiceType,
                        ProgressUpdates = job.Request.ProgressUpdates
                    };
                }
                else
                {
                    requestToSave = new ContactTransferRequest
                    {
                        SourceServer = job.Request.SourceServer,
                        SourceUser = job.Request.SourceUser,
                        SourcePassword = encryptionService.Encrypt(job.Request.SourcePassword) ?? "",
                        SourceOAuthToken = encryptionService.Encrypt(job.Request.SourceOAuthToken),
                        SourceUseOAuth = job.Request.SourceUseOAuth,
                        SourceServiceType = job.Request.SourceServiceType,
                        DestServer = job.Request.DestServer,
                        DestUser = job.Request.DestUser,
                        DestPassword = encryptionService.Encrypt(job.Request.DestPassword) ?? "",
                        DestOAuthToken = encryptionService.Encrypt(job.Request.DestOAuthToken),
                        DestUseOAuth = job.Request.DestUseOAuth,
                        DestServiceType = job.Request.DestServiceType,
                        ProgressUpdates = job.Request.ProgressUpdates
                    };
                }

                var persistedJob = new PersistedContactJobModel
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
                logger.LogInformation("Saved contact job {JobId} to disk", job.JobId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save contact job {JobId} to disk", job.JobId);
            }
        }

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly EncryptionService encryptionService;
        private readonly string jobStoragePath;
        private readonly ILogger<ContactJobPersistenceService> logger;

        private string GetJobFilePath(string jobId)
        {
            return Path.Combine(jobStoragePath, $"{jobId}.json");
        }
    }
}
