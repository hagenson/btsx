namespace Btsx
{
    public class MailMigrationRequest : MigrationRequest<MailMoverOptions>
    {        
        override public void Restarting()
        {
            if (Options != null && Options.DuplicateHandling == DuplicateHandling.CreateDuplicate)
                Options.DuplicateHandling = DuplicateHandling.Skip;
        }
    }
}
