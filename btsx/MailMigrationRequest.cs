namespace Btsx
{
    /// <summary>
    /// Encapsulates a request to migrate email from one service to another.
    /// </summary>
    public class MailMigrationRequest : MigrationRequest<MailMoverOptions>
    {
        /// <summary>
        /// Sets the DuplicateHanding option to Skip if it was previously CreateDuplicate.
        /// </summary>
        override public void Restarting()
        {
            if (Options != null && Options.DuplicateHandling == DuplicateHandling.CreateDuplicate)
                Options.DuplicateHandling = DuplicateHandling.Skip;
        }
    }
}
