namespace Btsx
{
    public class ContactMigrationRequest: MigrationRequest<ContactMoverOptions>
    {
        public override void Restarting()
        {
            if (Options != null && Options.DuplicateHandling == DuplicateHandling.CreateDuplicate)
                Options.DuplicateHandling = DuplicateHandling.Skip;
        }
    }
}
