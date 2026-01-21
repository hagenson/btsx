namespace Btsx
{
    /// <summary>
    /// Encapsulates statics for a mail migration job.
    /// </summary>
    public class MigrationStats
    {
        /// <summary>
        /// Number of messages that could not be copied.
        /// </summary>
        public int FailedMessages { get; set; }

        /// <summary>
        /// Number of messages that were skipped.
        /// </summary>
        /// <remarks>
        /// Messages may be skipped because a copy already exists in the destination account.
        /// </remarks>
        public int SkippedMessages { get; set; }

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