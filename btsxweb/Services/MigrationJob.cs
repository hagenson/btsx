using Btsx;

namespace BtsxWeb.Services
{
    /// <summary>
    /// Encapsulates the data for a migration job.
    /// </summary>
    public class MigrationJob: IJob
    {
        /// <inheritdoc/>
        public DateTime? EndTime { get; set; }

        /// <inheritdoc/>
        public bool IsCompleted { get; set; }

        /// <inheritdoc/>
        public string Id { get; set; } = "";

        /// <inheritdoc/>
        public int Progress { get; set; }

        /// <inheritdoc/>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public MigrationStats? Statistics { get; set; }

        /// <inheritdoc/>
        public string Status { get; set; } = "";

        /// <inheritdoc/>
        public string StatusType { get; set; } = "Info";

        /// <inheritdoc/>
        public MigrationRequest Request { get; set; } = default!;
    }
}