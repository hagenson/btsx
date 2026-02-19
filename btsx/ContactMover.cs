using System.Reflection;

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
                string? name = GetDisplayName(contact);
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
                    var existing = await dest.MatchContactsAsync(contact, cancellationToken);
                    if (existing.Count > 0)
                    {
                        switch (Options?.DuplicateHandling)
                        {
                            case DuplicateHandling.Skip:
                                DoStatus($"{name} already exists. Skipping.", true, StatusType.Info);
                                stats.SkippedMessages++;
                                break;
                            case DuplicateHandling.Overwrite:
                                DoStatus($"{name} already exists. Overwriting.", true, StatusType.Info);
                                // Put the contact in the same groups as the original
                                var groups = existing.SelectMany(e => e.Categories ?? new List<string>())
                                    .Distinct()
                                    .ToList();
                                contact.Categories = groups;
                                success = await dest.UploadContactAsync(contact, cancellationToken);

                                if (success)
                                {
                                    foreach (var del in existing)
                                    {
                                        if (!await dest.DeleteContactAsync(del, cancellationToken))
                                            DoStatus($"Unable to delete existing contact {GetDisplayName(del)} while merging. A duplicate has been created.", true, StatusType.Warning);
                                    }
                                }

                                if (success)
                                    stats.SuccessfulMessages++;
                                else
                                    stats.FailedMessages++;
                                break;
                            case DuplicateHandling.Merge:
                                DoStatus($"{name} already exists. Merging.", true, StatusType.Info);
                                var mergeTo = existing[0];
                                MergeContact(contact, mergeTo);
                                success = await dest.UpdateContactAsync(mergeTo, cancellationToken);
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
                        // Do we need to change the folder(s)
                        if (!string.IsNullOrEmpty(Options?.ImportFolderName))
                        {
                            if (contact.Categories == null)
                            {
                                contact.Categories = new List<string> { Options.ImportFolderName };
                            }
                            else if (contact.Categories.Count == 0)
                            {
                                contact.Categories.Add(Options.ImportFolderName);
                            }
                            else
                            {
                                for (int i = 0; i < contact.Categories.Count; i++)
                                {
                                    contact.Categories[i] = $"{Options.ImportFolderName} - {contact.Categories[i]}";
                                }
                            }
                        }
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

        private static string? GetDisplayName(IContactData contact)
        {
            return contact.FormattedName
                ?? contact.EmailAddresses?.FirstOrDefault()
                ?? contact.PhoneNumbers?.FirstOrDefault();
        }

        private void MergeContact(IContactData source, IContactData dest)
        {
            // Copy missing string properties from source to dest
            foreach (var prop in typeof(IContactData).GetProperties().Where(p => p.PropertyType == typeof(string)
                && p.CanRead && p.CanWrite))
            {
                MergeStringField(prop, source, dest);
            }

            // Merge any string collections
            foreach (var prop in typeof(IContactData).GetProperties().Where(p => p.PropertyType == typeof(List<string>)
                && p.Name != nameof(IContactData.Categories)
                && p.CanRead && p.CanWrite))
            {
                MergeStringCollectionField(prop, source, dest);
            }

            // Merge any date time properties
            foreach (var prop in typeof(IContactData).GetProperties().Where(p => p.PropertyType == typeof(DateTime?)
                && p.CanRead && p.CanWrite))
            {
                MergeDateField(prop, source, dest);
            }

        }

        private void MergeStringField(PropertyInfo prop, IContactData source, IContactData dest)
        {
            var cur = (string?)prop.GetValue(dest);
            var upd = (string?)prop.GetValue(source);
            if (string.IsNullOrWhiteSpace(cur)
                    && !string.IsNullOrWhiteSpace(upd))
                prop.SetValue(dest, upd);
        }

        private void MergeDateField(PropertyInfo prop, IContactData source, IContactData dest)
        {
            var cur = (DateTime?)prop.GetValue(dest);
            var upd = (DateTime?)prop.GetValue(source);
            if (upd.HasValue
                    && !cur.HasValue)
                prop.SetValue(dest, upd);
        }

        private void MergeStringCollectionField(PropertyInfo prop, IContactData source, IContactData dest)
        {
            var cur = (List<string>?)prop.GetValue(dest);
            var upd = (List<string>?)prop.GetValue(source);
            if (cur == null
                || upd == null)
                return;

            foreach (var address in upd)
            {
                if (!cur.Any(c => string.Equals(c, address, StringComparison.OrdinalIgnoreCase)))
                    cur.Add(address);
            }
        }

    }
}
