using Btsx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Btsx
{
    public static class ContactServiceFactory
    {
        public static IContactService CreateContactService(Creds creds)
        {
            var name = $"{creds.Implementor}ContactsService";
            var type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => t.Name ==  name);
            if (type == null)
                throw new TypeLoadException($"Type {name} not found.");
            var service = (IContactService)Activator.CreateInstance(type, [creds])!;
            return service;
        }

    }
}
