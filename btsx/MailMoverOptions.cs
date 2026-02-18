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
        /// When true, no emails are copied, only the destination folders are created.
        /// </summary>
        public bool FoldersOnly { get; set; }

        /// <summary>
        /// When true, for each email message copied, an attempt will be made to see if it already exists on the destination server
        /// by trying to match the Message ID header.
        /// </summary>
        /// <remarks>
        /// If false, no checks will be made to see if the email message exists. This may lead to the duplication of
        /// emails in the destination account if multiple migration attempts are made.
        /// </remarks>
        public bool ReplaceExisting { get; set; }
    }
}