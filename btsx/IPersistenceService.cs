using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btsx
{
    public interface IPersistenceService
    {
        Task CleanupOldJobsAsync(CancellationToken cancellationToken);
        Task DeleteJobAsync(string jobId, CancellationToken cancellationToken);
        Task<List<IJob>> GetIncompleteJobsAsync(CancellationToken cancellationToken);
        Task<List<IJob>> LoadAllJobsAsync(CancellationToken cancellationToken);
        Task<IJob?> LoadJobAsync(string jobId, CancellationToken cancellationToken);
        Task SaveJobAsync(IJob job, CancellationToken cancellationToken);
        Task ClearProtectedPropertiesAsync(IJob job, CancellationToken cancellationToken);
    }

}
