namespace Btsx
{
    /// <summary>
    /// Encapsulates statics for a data migration job.
    /// </summary>
    public class MigrationStats
    {
        /// <summary>
        /// Number of items that could not be copied.
        /// </summary>
        public int FailedItems { get; set; }

        /// <summary>
        /// Number of items that were skipped.
        /// </summary>
        /// <remarks>
        /// Items may be skipped because a copy already exists in the destination account, depending on
        /// and duplicate handling options for a migration job.
        /// </remarks>
        public int SkippedItems { get; set; }

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