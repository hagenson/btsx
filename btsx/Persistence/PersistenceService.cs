using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Btsx.Persistence
{
    public class PersistenceService : IPersistenceService
    {
        public PersistenceService(
            IEncryptionService encryption,
            IOptions<PersistenceSettings> settings,
            ILogger<PersistenceService> log)
        {
            this.encryption = encryption;
            jobStoragePath = settings.Value.StorageDirectory;
            this.log = log;
        }

        /// <inheritdoc/>
        public async Task CleanupOldJobsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var allJobs = await LoadAllJobsAsync(cancellationToken);
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                var oldJobs = allJobs.Where(j => j.StartTime < sevenDaysAgo).ToList();

                foreach (var job in oldJobs)
                {
                    await DeleteJobAsync(job.Id, cancellationToken);
                }

                if (oldJobs.Count > 0)
                    log.LogInformation("Cleaned up {Count} old jobs", oldJobs.Count);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to cleanup old jobs");
            }
        }

        /// <inheritdoc/>
        public Task DeleteJobAsync(string jobId, CancellationToken cancellationToken)
        {
            try
            {
                var filePath = GetJobFilePath(jobId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    log.LogInformation("Deleted job {JobId} from disk", jobId);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to delete job {JobId} from disk", jobId);
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<List<IJob>> GetIncompleteJobsAsync(CancellationToken cancellationToken)
        {
            var allJobs = await LoadAllJobsAsync(cancellationToken);
            return allJobs.Where(j => !j.IsCompleted).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<IJob>> LoadAllJobsAsync(CancellationToken cancellationToken)
        {
            var jobs = new List<IJob>();

            try
            {
                var files = Directory.GetFiles(jobStoragePath, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var job = await LoadJobInternalAsync(file);
                        if (job != null)
                            jobs.Add(job);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Failed to load job from file {File}", file);
                    }
                }
                log.LogInformation("Loaded {Count} jobs from disk", jobs.Count);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to load jobs from disk");
            }

            return jobs;
        }

        /// <inheritdoc/>
        public Task<IJob?> LoadJobAsync(string jobId, CancellationToken cancellationToken)
        {
            var filePath = GetJobFilePath(jobId);
            if (!File.Exists(filePath))
                return Task.FromResult<IJob?>(null);
            return LoadJobInternalAsync(filePath);
        }

        /// <inheritdoc/>
        public Task ClearProtectedPropertiesAsync(IJob job, CancellationToken cancellationToken)
        {
            // NULL the protected properties
            WalkProtectedProperties(job, s => null);
            return SaveJobInternalAsync(job, job, cancellationToken);
        }

        /// <inheritdoc/>
        public Task SaveJobAsync(IJob job, CancellationToken cancellationToken)
        {

            // Copy the object
            string data = JsonSerializer.Serialize(job, job.GetType())!;
            var copy = JsonSerializer.Deserialize(data, job.GetType())!;

            // Encrypt any protected properties
            WalkProtectedProperties(copy, s => encryption.Encrypt(s));
            return SaveJobInternalAsync(job, copy, cancellationToken);
        }

        private Task SaveJobInternalAsync(IJob job, object copy, CancellationToken cancellationToken)
        {
            // Add the type property
            string data = JsonSerializer.Serialize(copy, job.GetType(), jsopt)!;
            var idx = data.IndexOf('{');
            data = data.Substring(idx + 1);
            data = "{" + $"{Environment.NewLine}  \"Type\":\"{job.GetType().AssemblyQualifiedName}\", "
                + data;

            // Save the data
            var filePath = GetJobFilePath(job.Id);
            return File.WriteAllTextAsync(filePath, data, cancellationToken);
        }

        private readonly IEncryptionService encryption;
        private readonly string jobStoragePath;
        private readonly ILogger<PersistenceService> log;

        private void WalkProtectedProperties(object obj, Func<string?, string?> crypt)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.CustomAttributes.Any(a => a.AttributeType == typeof(ProtectedAttribute))
                    && prop.CanRead
                    && prop.CanWrite)
                {
                    // Decrypt non-null strings
                    string? val = (string?)prop.GetValue(obj);
                    if (val != null)
                    {
                        val = crypt(val);
                        prop.SetValue(obj, val);
                    }
                }
                else if ((prop.PropertyType.Namespace != null
                        && prop.PropertyType.Namespace != "System"
                        && !prop.PropertyType.Namespace.StartsWith("System.")))
                {
                    // Descend to non-null child properties
                    var val = prop.GetValue(obj);
                    if (val != null)
                    {
                        WalkProtectedProperties(val, crypt);
                    }
                }
            }
        }

        private string GetJobFilePath(string jobId)
        {
            return Path.Combine(jobStoragePath, $"{jobId}.json");
        }

        private async Task<IJob?> LoadJobInternalAsync(string file)
        {
            var json = await File.ReadAllTextAsync(file);
            // Read the header
            var persistedJob = JsonSerializer.Deserialize<PersistedJob>(json);
            if (persistedJob == null)
                return null;
            // Now deserialise to the specific job type
            var type = Type.GetType(persistedJob.Type);
            var job = (IJob?)JsonSerializer.Deserialize(json, type);
            if (job == null)
                return null;

            // Decrypt any encrypted data
            WalkProtectedProperties(job, s => encryption.Decrypt(s));

            return job;
        }

        private static JsonSerializerOptions jsopt = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
    }
}