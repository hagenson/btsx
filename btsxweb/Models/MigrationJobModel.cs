namespace BtsxWeb.Models
{
    /// <summary>
    /// Encapsulates
    /// </summary>
    public class MigrationJobModel
    {
        /// <summary>
        /// Date and time the job completed.
        /// </summary>
        public DateTime? EndTime { get; set; }

        public int FailedMessages { get; set; }
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
        /// Number of mail messages skipped.
        /// </summary>
        public int SkippedMessages { get; set; }

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
        /// Number of messages successfully copied.
        /// </summary>
        public int SuccessfulMessages { get; set; }

        /// <summary>
        /// Total number of messages processed.
        /// </summary>
        public int TotalMessages { get; set; }
    }
}