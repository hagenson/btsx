using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btsx
{
    public interface IMover
    {
        Task ExecuteAsync(CancellationToken cancellationToken);

        event StatusEvent? StatusUpdate;

        public bool ProgressUpdates { get; set; }

        public MigrationStats? Statistics { get; }

        Task<bool> TestAuthenticationAsync(Creds creds, CancellationToken cancellationToken);
    }

    public interface IMover<TCredentials, TOptions>: IMover where TCredentials: class where TOptions: class
    {
        TCredentials? SourceCredentials { get; set; }
        TCredentials? DestinationCredentials { get; set; }
        TOptions? Options { get; set; }

    }
}
