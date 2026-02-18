namespace Btsx
{
    /// <summary>
    /// Contains the logic to transfer contacts from one account to another.
    /// </summary>
    public class ContactMover: MoverBase, IMover<Creds, ContactMoverOptions>
    {

        /// <summary>
        /// Specifies the destination account.
        /// </summary>
        public Creds? DestinationCredentials { get; set; }

        
        /// <summary>
        /// The credentials for the source account.
        /// </summary>
        public Creds? SourceCredentials { get; set; }

        public ContactMoverOptions? Options { get; set; }

        /// <summary>
        /// Tests that the provided credentials will successfully authenticate.
        /// </summary>
        /// <param name="creds">Account credentials to test.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the account was authenticated successfully.</returns>
        public override Task<bool> TestAuthenticationAsync(
            Creds creds,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IContactService service = ContactServiceFactory.CreateContactService(creds);
                return service.TestConnectionAsync(cancellationToken);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        
        /// <summary>
        /// Runs the configured contact transfer job.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Awaitable task.</returns>
        public async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (SourceCredentials == null)
                throw new InvalidOperationException($"{nameof(SourceCredentials)} must be specified.");
            if (DestinationCredentials == null)
                throw new InvalidOperationException($"{nameof(DestinationCredentials)} must be specified.");

            totalItems = 0;
            completedItems = 0;
            progress = 0;

            var source = ContactServiceFactory.CreateContactService(SourceCredentials);
            var dest = ContactServiceFactory.CreateContactService(DestinationCredentials);

            DoStatus($"Listing contact groups from {SourceCredentials.Server}...", false, StatusType.Info);

            DoStatus($"Listing contacts from {SourceCredentials.Server}...", false, StatusType.Info);
            var contacts = (await source.ListContactsAsync(cancellationToken))
                .Concat(await source.ListCollectedContactsAsync(cancellationToken))
                .ToList();
            totalItems = contacts.Count;
            var stats = new MigrationStats
            {
                TotalMessages = totalItems,
            };
            foreach (var contact in contacts)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                var name = contact.FormattedName
                    ?? contact.EmailAddresses?.FirstOrDefault()
                    ?? contact.PhoneNumbers?.FirstOrDefault();
                DoStatus($"Moving {name}...", true, StatusType.Info);
                
                bool success;
                if (Options?.DuplicateHandling == DuplicateHandling.CreateDuplicate)
                {
                    success = await dest.UploadContactAsync(contact, cancellationToken);
                    if (success)
                        stats.SuccessfulMessages++;
                    else
                        stats.FailedMessages++;
                }
                else
                {
                    var exists = await dest.ContactExistsAsync(contact, cancellationToken);
                    if (exists)
                    {
                        switch (Options?.DuplicateHandling)
                        {
                            case DuplicateHandling.Skip:
                                DoStatus($"{name} already exists. Skipping.", true, StatusType.Info);
                                stats.SkippedMessages++;
                                break;
                            case DuplicateHandling.Overwrite:
                                DoStatus($"{name} already exists. Overwriting.", true, StatusType.Info);
                                var deleted = await dest.DeleteContactAsync(contact, cancellationToken);
                                if (deleted)
                                {
                                    success = await dest.UploadContactAsync(contact, cancellationToken);
                                    if (success)
                                        stats.SuccessfulMessages++;
                                    else
                                        stats.FailedMessages++;
                                }
                                else
                                {
                                    stats.FailedMessages++;
                                }
                                break;
                            case DuplicateHandling.Merge:
                                DoStatus($"{name} already exists. Merging.", true, StatusType.Info);
                                success = await dest.UpdateContactAsync(contact, cancellationToken);
                                if (success)
                                    stats.SuccessfulMessages++;
                                else
                                    stats.FailedMessages++;
                                break;
                            default:
                                DoStatus($"{name} already exists. Skipping.", true, StatusType.Info);
                                stats.SkippedMessages++;
                                break;
                        }
                    }
                    else
                    {
                        success = await dest.UploadContactAsync(contact, cancellationToken);
                        if (success)
                            stats.SuccessfulMessages++;
                        else
                            stats.FailedMessages++;
                    }
                }
                completedItems++;
            }

            DoStatus("Transfer complete.", true, StatusType.Info);
            Statistics = stats;
        }


    }
}
