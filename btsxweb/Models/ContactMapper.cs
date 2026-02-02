using BtsxWeb.Services;

namespace BtsxWeb.Models
{
    /// <summary>
    /// Provides object mapping helpers for contact transfer jobs.
    /// </summary>
    public class ContactMapper
    {
        /// <summary>
        /// Maps a contact transfer job to an object suitable for JSON serialisation.
        /// </summary>
        /// <param name="src">Contact transfer job to map.</param>
        /// <returns>Serialisable model.</returns>
        public ContactTransferJobModel Map(ContactTransferJob src)
        {
            var stats = src.Statistics ?? new Btsx.MigrationStats();
            return new ContactTransferJobModel
            {
                EndTime = src.EndTime,
                FailedContacts = stats.FailedMessages,
                IsCompleted = src.IsCompleted,
                JobId = src.JobId,
                Progress = src.Progress,
                SkippedContacts = stats.SkippedMessages,
                SourceServer = src.Request?.SourceServer,
                StartTime = src.StartTime,
                Status = src.Status,
                StatusType = src.StatusType,
                SuccessfulContacts = stats.SuccessfulMessages,
                TotalContacts = stats.TotalMessages,
                ProgressUpdates = src.Request?.ProgressUpdates == true
            };
        }
    }
}
