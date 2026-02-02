using MailKit;
using MailKit.Net.Imap;

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
        public static async Task<bool> TestAuthenticationAsync(Creds creds, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = new ImapClient())
                {
                    await ConnectHost(client, creds, cancellationToken);
                    await client.DisconnectAsync(true, cancellationToken);
                    return true;
                }
            }
            catch
            {
                return false;
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

            using (var srcHost = new ImapClient())
            using (var dstHost = new ImapClient())
            {
                DoStatus($"Connecting to source: {SourceCredentials.Server}...", false, StatusType.Info);
                await ConnectHost(srcHost, SourceCredentials, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    DoStatus("Connected to source server", false, StatusType.Info);

                    DoStatus($"Connecting to destination: {DestCredentials.Server}...", false, StatusType.Info);
                    await ConnectHost(dstHost, DestCredentials, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    DoStatus("Connected to destination server", false, StatusType.Info);

                    var stats = new MigrationStats();

                    DoStatus("Transfer complete.", true, StatusType.Info);
                    Statistics = stats;
                }
                finally
                {
                    if (srcHost.IsConnected)
                        await srcHost.DisconnectAsync(true, CancellationToken.None);
                    if (dstHost.IsConnected)
                        await dstHost.DisconnectAsync(true, CancellationToken.None);
                }
            }
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

        private static async Task ConnectHost(ImapClient host, Creds creds, CancellationToken cancellationToken)
        {
            await host.ConnectAsync(creds.Server, 993, true, cancellationToken);

            if (creds.UseOAuth && !string.IsNullOrEmpty(creds.OAuthToken))
            {
                var oauth2 = new MailKit.Security.SaslMechanismOAuth2(creds.User, creds.OAuthToken);
                await host.AuthenticateAsync(oauth2, cancellationToken);
            }
            else
            {
                await host.AuthenticateAsync(creds.User, creds.Password, cancellationToken);
            }
        }

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
