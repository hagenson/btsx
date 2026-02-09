using Btsx;
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

        Task<List<ContactData>> ListContactsAsync(CancellationToken cancellationToken = default);
        Task<bool> ContactExistsAsync(string filename, CancellationToken cancellationToken);
        Task<bool> UploadContactAsync(string vcard, string filename, CancellationToken cancellationToken);
    }
}
