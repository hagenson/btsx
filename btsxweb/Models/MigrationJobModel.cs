namespace BtsxWeb.Models
{
    /// <summary>
    /// Encapsulates information about a migration job for UI display.
    /// </summary>
    public class MigrationJobModel
    {
        /// <summary>
        /// Date and time the job completed.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// The number of items that could not be moved.
        /// </summary>
        public int FailedItems { get; set; }

        /// <summary>
        /// True when the job has completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// The ID that identifies the migration job.
        /// </summary>
        public string JobId { get; set; } = "";

        /// <summary>
        /// Percentage progress of the executing job.
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// True if progress updates requested.
        /// </summary>
        public bool ProgressUpdates { get; set; }

        /// <summary>
        /// Number of items skipped.
        /// </summary>
        public int SkippedItems { get; set; }

        /// <summary>
        /// Host name of the source server.
        /// </summary>
        public string? SourceServer { get; set; }

        /// <summary>
        /// Date and time the migration job started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Text description of migration job status.
        /// </summary>
        public string Status { get; set; } = "";

        /// <summary>
        /// Status update type.
        /// </summary>
        public string StatusType { get; set; } = "";

        /// <summary>
        /// Number of items successfully copied.
        /// </summary>
        public int SuccessfulItems { get; set; }

        /// <summary>
        /// Total number of items processed.
        /// </summary>
        public int TotalItems { get; set; }
    }
}