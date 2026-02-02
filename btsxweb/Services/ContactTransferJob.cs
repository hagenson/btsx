using Btsx;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Encapsulates the data for a contact transfer job.
    /// </summary>
    public class ContactTransferJob
    {
        /// <summary>
        /// Cancellation token source that can be used to cancel the running job.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();

        /// <summary>
        /// Date and time the job terminated if is it complete.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// True when the job has completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Unique Identifier for the job.
        /// </summary>
        public string JobId { get; set; } = "";

        /// <summary>
        /// Percentage completion of the job.
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Contact transfer job request parameters from which the job was created.
        /// </summary>
        public ContactTransferRequest Request { get; set; } = new();

        /// <summary>
        /// Date and time the job started running.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Statistics on contacts processed by the job. 
        /// </summary>
        public MigrationStats? Statistics { get; set; }

        /// <summary>
        /// Last status update text.
        /// </summary>
        public string Status { get; set; } = "";

        /// <summary>
        /// Last status update type.
        /// </summary>
        public string StatusType { get; set; } = "Info";
    }
}
