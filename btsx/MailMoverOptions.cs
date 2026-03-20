namespace Btsx
{
    /// <summary>
    /// Encapsulates options for moving mail messages from one account to another.
    /// </summary>
    public class MailMoverOptions
    {
        /// <summary>
        /// If true, emails will be deleted from the source account after being copied to the destination account.
        /// </summary>
        public bool DeleteSource { get; set; }

        /// <summary>
        /// Specifies what to do when an email with the same Message ID already exists in the same folder at the destination.
        /// </summary>
        public DuplicateHandling DuplicateHandling { get; set; } = DuplicateHandling.Overwrite;

        /// <summary>
        /// When true, no emails are copied, only the destination folders are created.
        /// </summary>
        public bool FoldersOnly { get; set; }
    }
}