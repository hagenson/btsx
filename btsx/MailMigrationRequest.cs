namespace Btsx
{
    public class MailMigrationRequest : MigrationRequest<MailMoverOptions>
    {        
        override public void Restarting()
        {
            if (Options != null)
                Options.ReplaceExisting = true;
        }
    }
}
