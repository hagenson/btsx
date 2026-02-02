namespace BtsxWeb.Models
{
    /// <summary>
    /// Encapsulates contact transfer job status for serialization.
    /// </summary>
    public class ContactTransferJobModel
    {
        /// <summary>
        /// Date and time the job completed.
        /// </summary>
        public DateTime? EndTime { get; set; }

        public int FailedContacts { get; set; }

        /// <summary>
        /// True when the job has completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// The ID that identifies the contact transfer job.
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
        /// Number of contacts skipped.
        /// </summary>
        public int SkippedContacts { get; set; }

        /// <summary>
        /// Host name of the source server.
        /// </summary>
        public string? SourceServer { get; set; }

        /// <summary>
        /// Date and time the contact transfer job started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Text description of contact transfer job status.
        /// </summary>
        public string Status { get; set; } = "";

        /// <summary>
        /// Status update type.
        /// </summary>
        public string StatusType { get; set; } = "";

        /// <summary>
        /// Number of contacts successfully transferred.
        /// </summary>
        public int SuccessfulContacts { get; set; }

        /// <summary>
        /// Total number of contacts processed.
        /// </summary>
        public int TotalContacts { get; set; }
    }
}
