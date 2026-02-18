using Btsx;

namespace BtsxWeb.Services
{
    public class MoverFactory : IMoverFactory
    {
        public IMover CreateAuthenticator(MigrationType migrationType)
        {
            switch(migrationType)
            {
                case MigrationType.Mail:
                    return new MailMover();
                case MigrationType.Contacts:
                    return new ContactMover();
                default:
                    throw new NotSupportedException($"Migration type {migrationType} is not supported.");
            }
        }

        public IMover CreateMover(MigrationRequest request)
        {
            return CreateMoverInternal((dynamic)request);
        }

        private IMover CreateMoverInternal(MailMigrationRequest request)
        {
            return new MailMover
            {
                SourceCredentials = request.SourceCredentials,
                DestinationCredentials = request.DestinationCredentials,
                Options = request.Options,
                ProgressUpdates = request.ProgressUpdates == true,
            };
        }

        private IMover CreateMoverInternal(ContactMigrationRequest request)
        {
            return new ContactMover
            {
                SourceCredentials = request.SourceCredentials,
                DestinationCredentials = request.DestinationCredentials,
                Options = request.Options,
            };
        }
    }
}
