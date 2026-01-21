namespace BtsxWeb.Models
{
    /// <summary>
    /// Encapsulates migration job statistics to send to the front end.
    /// </summary>
    public class JobStatisticsModel
    {
        /// <summary>
        /// Number of messages that have failed to be copied.
        /// </summary>
        public int FailedMessages { get; set; }

        /// <summary>
        /// Number of messages skipped.
        /// </summary>
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