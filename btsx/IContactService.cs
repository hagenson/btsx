namespace Btsx
{
    /// <summary>
    /// Defines operations for accessing and manipulating contact data on a remote service.
    /// </summary>
    public interface IContactService
    {
        /// <summary>
        /// Creates a new contact on the remote server.
        /// </summary>
        /// <param name="contact">Contact object create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the contact was created remotely.</returns>
        Task<bool> CreateContactAsync(IContactData contact, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously deletes the specified contact from the data store.
        /// </summary>
        /// <param name="contact">The contact to delete. Cannot be null.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>`True if the operation succeeds.</returns>
        Task<bool> DeleteContactAsync(IContactData contact, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously retrieves a list of collected contacts.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of collected contacts.</returns>
        /// <remarks>Some cloud providers keep a list of sent and received email addresses as a read-only contact list.
        /// Often only the email address will be available for each contact.
        /// If a particular provider does not have this functionality, the implementer should return an empty list.
        /// </remarks>
        Task<List<IContactData>> ListCollectedContactsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves a list of contact data objects from the underlying data source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of contact data objects.</returns>
        Task<List<IContactData>> ListContactsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously finds and returns a list of existing contacts that match the specified contact data.
        /// </summary>
        /// <param name="contact">The contact data to use as the basis for matching. This parameter cannot be null.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of matching contact data objects.</returns>
        /// <remarks>The matching logic is left to the implementer, but it is assumed that if an email address is the same, there is a match.</remarks>
        Task<List<IContactData>> MatchContactsAsync(IContactData contact, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to authenticate with the remotes service..
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the authentication attempt worked.</returns>
        /// <remarks>
        /// This operation is intended for services that do not use OAuth authentication. The implementing object should be
        /// initialised with connection credentials before this operation is invoked.
        /// </remarks>
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Updates the specified contact on the remote server.
        /// </summary>
        /// <param name="contact">Contact retrieved with a previous call to <see cref="ListContactsAsync(CancellationToken)"/> or
        /// <see cref="MatchContactsAsync(IContactData, CancellationToken)"/> that has had it properties updated.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the update succeeded.</returns>
        Task<bool> UpdateContactAsync(IContactData contact, CancellationToken cancellationToken);
    }
}