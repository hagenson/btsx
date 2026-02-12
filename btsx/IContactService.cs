using Btsx.Google;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btsx
{
    public interface IContactService
    {
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken);

        Task<List<IContactData>> ListContactsAsync(CancellationToken cancellationToken = default);
        Task<List<IContactData>> ListCollectedContactsAsync(CancellationToken cancellationToken = default);
        Task<bool> ContactExistsAsync(IContactData contact, CancellationToken cancellationToken);
        Task<bool> UploadContactAsync(IContactData contact, CancellationToken cancellationToken);
    }
}
