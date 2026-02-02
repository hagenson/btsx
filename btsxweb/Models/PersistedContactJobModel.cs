using Btsx;
using BtsxWeb.Services;

namespace BtsxWeb.Models
{
    /// <summary>
    /// Encapsulates the persistent data for a contact transfer job.
    /// </summary>
    public class PersistedContactJobModel
    {
        /// <summary>
        /// End time if the job has completed.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// True if the job has completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Identified for the job.
        /// </summary>
        public string JobId { get; set; } = "";

        /// <summary>
        /// Percentage progress of the job.
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Original contact transfer request parameters.
        /// </summary>
        public ContactTransferRequest Request { get; set; } = new();

        /// <summary>
        /// Date and time the job was started.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Statistics on the number of contacts processed.
        /// </summary>
        public MigrationStats? Statistics { get; set; }

        /// <summary>
        /// Last status text.
        /// </summary>
        public string Status { get; set; } = "";

        /// <summary>
        /// Last status type.
        /// </summary>
        public string StatusType { get; set; } = "Info";
    }
}
