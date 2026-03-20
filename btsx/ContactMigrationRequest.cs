namespace Btsx
{
    /// <summary>
    /// Encapsulates a request to migrated contacts.
    /// </summary>
    public class ContactMigrationRequest : MigrationRequest<ContactMoverOptions>
    {
        /// <summary>
        /// Sets the duplicate handling option to Skip if it was previously CreateDuplicate.
        /// </summary>
        public override void Restarting()
        {
            if (Options != null && Options.DuplicateHandling == DuplicateHandling.CreateDuplicate)
                Options.DuplicateHandling = DuplicateHandling.Skip;
        }
    }
}