using BtsxWeb.Services;

namespace BtsxWeb.Models
{
    /// <summary>
    /// Provides object mapping helpers.
    /// </summary>
    public class Mapper
    {
        /// <summary>
        /// Maps a migration job to an object suitable for JSON serialisation.
        /// </summary>
        /// <param name="src">Migration job to map.</param>
        /// <returns>Serialisable model.</returns>
        public MigrationJobModel Map(MigrationJob src)
        {
            var stats = src.Statistics ?? new Btsx.MigrationStats();
            return new MigrationJobModel
            {
                EndTime = src.EndTime,
                FailedMessages = stats.FailedMessages,
                IsCompleted = src.IsCompleted,                
                JobId = src.JobId,
                Progress = src.Progress,
                SkippedMessages = stats.SkippedMessages,
                SourceServer = src.Request?.SourceServer,
                StartTime = src.StartTime,
                Status = src.Status,
                StatusType = src.StatusType,
                SuccessfulMessages = stats.SuccessfulMessages,
                TotalMessages = stats.TotalMessages,
                ProgressUpdates = src.Request?.ProgressUpdates == true
            };
        }
    }
}