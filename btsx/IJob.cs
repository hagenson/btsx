namespace Btsx
{
    /// <summary>
    /// Defines the basic information common to all migration jobs.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Date and time the job terminated.
        /// </summary>
        DateTime? EndTime { get; set; }

        /// <summary>
        /// Unique identifier for the job.
        /// </summary>
        string Id { get; set; }
        
        /// <summary>
        /// Indicates whether the job has finished or not.
        /// </summary>
        bool IsCompleted { get; set; }

        /// <summary>
        /// Percentage completion of the job.
        /// </summary>
        int Progress { get; set; }

        /// <summary>
        /// The original request that created the job.
        /// </summary>
        MigrationRequest Request { get; set; }

        /// <summary>
        /// Date and time the job started execution.
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        /// Statistics about how many data objects were transferred by the job etc.
        /// </summary>
        MigrationStats? Statistics { get; set; }

        /// <summary>
        /// Last status message of the running job.
        /// </summary>
        string Status { get; set; }

        /// <summary>
        /// Last status message type of the running job.
        /// </summary>
        string StatusType { get; set; }
    }
}