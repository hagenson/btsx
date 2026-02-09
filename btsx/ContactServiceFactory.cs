using Btsx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btsx
{
    public static class ContactServiceFactory
    {
        public static IContactService CreateContactService(Creds creds)
        {
            var type = Type.GetType($"Btsx.{creds.Implementor}ContactsService", true)!;
            var service = (IContactService)Activator.CreateInstance(type, [creds])!;
            return service;
        }

    }
}
