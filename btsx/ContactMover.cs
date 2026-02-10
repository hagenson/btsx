namespace Btsx
{
    /// <summary>
    /// Contains the logic to transfer contacts from one account to another.
    /// </summary>
    public class ContactMover
    {
        /// <summary>
        /// Event triggered to report status updates.
        /// </summary>
        public event StatusEvent StatusUpdate
        {
            add
            {
                statusUpdate += value;
            }
            remove
            {
                statusUpdate -= value;
            }
        }

        /// <summary>
        /// Specifies the destination account.
        /// </summary>
        public Creds? DestCredentials { get; set; }

        /// <summary>
        /// When true, completion percentage will be calculated and progress updates notified via the <see cref="StatusUpdate"/> event.
        /// </summary>
        public bool ProgressUpdates { get; set; }

        /// <summary>
        /// The credentials for the source account.
        /// </summary>
        public Creds? SourceCredentials { get; set; }

        /// <summary>
        /// Contains statistics for the executed migration job.
        /// </summary>
        public MigrationStats? Statistics { get; private set; }

        /// <summary>
        /// Tests that the provided credentials will successfully authenticate.
        /// </summary>
        /// <param name="creds">Account credentials to test.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the account was authenticated successfully.</returns>
        public static Task<bool> TestAuthenticationAsync(
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
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (SourceCredentials == null)
                throw new InvalidOperationException($"{nameof(SourceCredentials)} must be specified.");
            if (DestCredentials == null)
                throw new InvalidOperationException($"{nameof(DestCredentials)} must be specified.");

            totalContacts = 0;
            completedContacts = 0;
            progress = 0;

            var source = ContactServiceFactory.CreateContactService(SourceCredentials);
            var dest = ContactServiceFactory.CreateContactService(DestCredentials);

            DoStatus($"Listing contact groups from {SourceCredentials.Server}...", false, StatusType.Info);

            DoStatus($"Listing contacts from {SourceCredentials.Server}...", false, StatusType.Info);
            var contacts = await source.ListContactsAsync(cancellationToken);
            totalContacts = contacts.Count;
            var stats = new MigrationStats
            {
                TotalMessages = totalContacts,
            };
            foreach (var contact in contacts)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (contact.Person == null
                    || contact.Person.Names == null
                    || contact.Person.Names.Count == 0
                    || string.IsNullOrEmpty(contact.Person.Names[0].DisplayName))
                {
                    DoStatus($"Contact has no name, Skipping.", true, StatusType.Info);
                    stats.SkippedMessages++;
                }
                else
                {
                    var name = contact.Person.Names[0].DisplayName;
                    DoStatus($"Moving {name}...", true, StatusType.Info);
                    if (await dest.ContactExistsAsync(name, cancellationToken))
                    {
                        DoStatus($"{contact.ResourceName} already exists. Skipping.", true, StatusType.Info);
                        stats.SkippedMessages++;
                    }
                    else
                    {
                        await dest.UploadContactAsync(contact.VCard, name, cancellationToken);
                        stats.SuccessfulMessages++;
                    }
                }
                completedContacts++;
            }

            DoStatus("Transfer complete.", true, StatusType.Info);
            Statistics = stats;
            
        }

        /// <summary>
        /// Called to notify status updates.
        /// </summary>
        /// <param name="args">Migration job status.</param>
        protected virtual void OnProgressUpdate(StatusEventArgs args)
        {
            statusUpdate?.Invoke(this, args);
        }

        private int completedContacts;

        private int progress;

        private int totalContacts;

        private event StatusEvent? statusUpdate;


        private void DoStatus(string message, bool progress, StatusType type)
        {
            bool send = false;
            var prog = totalContacts > 0 
                ? (int)((decimal)completedContacts / (decimal)totalContacts * 100m)
                : 0;
            if (progress)
            {
                if (totalContacts > 0)
                {
                    if (prog != this.progress)
                    {
                        send = true;
                        this.progress = prog;
                    }
                }

                send = send || completedContacts % 10 == 0;
            }
            else
            {
                this.progress = prog;
                send = true;
            }

            if (send)
            {
                OnProgressUpdate(new StatusEventArgs { Percentage = this.progress, Status = message, Type = type });
            }
        }
    }
}
