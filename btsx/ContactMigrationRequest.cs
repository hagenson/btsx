namespace Btsx
{
    public class ContactMigrationRequest: MigrationRequest<ContactMoverOptions>
    {
        public override void Restarting()
        {
            if (Options != null)
                Options.ReplaceExisting = true;
        }
    }
}
