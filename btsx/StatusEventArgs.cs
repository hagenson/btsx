namespace Btsx
{
    /// <summary>
    /// Event delegate for migration job status updates.
    /// </summary>
    /// <param name="sender">The MaiMover instance that triggered the event.</param>
    /// <param name="e">The event arguments.</param>
    public delegate void StatusEvent(object sender, StatusEventArgs e);

    /// <summary>
    /// The kind of status event being notified.
    /// </summary>
    public enum StatusType
    {
        /// <summary>
        /// An informational message.
        /// </summary>
        Info,
        /// <summary>
        /// A warning message.
        /// </summary>
        Warning,
        /// <summary>
        /// An error message.
        /// </summary>
        Error
    }

    /// <summary>
    /// Encapsulates migration job status values.
    /// </summary>
    public class StatusEventArgs : EventArgs
    {
        /// <summary>
        /// Percentage complete.
        /// </summary>
        public int Percentage { get; set; }

        /// <summary>
        /// Status message from the MailMover.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// The type of status update being notified.
        /// </summary>
        public StatusType Type { get; set; }
    }
}