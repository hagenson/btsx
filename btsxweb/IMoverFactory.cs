using Btsx;

namespace BtsxWeb
{
    public interface IMoverFactory
    {
        IMover CreateMover(MigrationRequest request);
        
        IMover CreateAuthenticator(MigrationType migrationType);

    }
}
