using System.Reflection;

namespace Btsx
{
    /// <summary>
    /// Implements a factory to create <see cref="IContactService"/> implementers for supplied credentials.
    /// </summary>
    public static class ContactServiceFactory
    {
        /// <summary>
        /// Instantiates a contact service based on the <see cref="Creds.Implementer"/> property
        /// of the supplied credentials.
        /// </summary>
        /// <param name="creds">Credentials to instantiate service for.</param>
        /// <returns>Initialised contact service.</returns>
        public static IContactService CreateContactService(Creds creds)
        {
            var name = $"{creds.Implementer}ContactsService";
            var type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => t.Name == name);
            if (type == null)
                throw new TypeLoadException($"Type {name} not found.");
            var service = (IContactService)Activator.CreateInstance(type, [creds])!;
            return service;
        }
    }
}