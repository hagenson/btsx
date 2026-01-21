using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace Btsx
{
    /// <summary>
    /// Contains the logic to move of copy emails from one account to another.
    /// </summary>
    public class MailMover
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
        /// If true, emails will be deleted from the source account after being copied to the destination account.
        /// </summary>
        public bool DeleteSource
        {
            get
            {
                return deleteSource;
            }

            set
            {
                deleteSource = value;
                SetSourceAccess();
            }
        }

        /// <summary>
        /// Specifies the destination email account.
        /// </summary>
        public Creds? DestCredentials { get; set; }

        /// <summary>
        /// When true, no emails are copied, only the destination folders are created.
        /// </summary>
        public bool FoldersOnly { get; set; }

        /// <summary>
        /// When true, completion percentage will be calculated and progress updates notified via the <see cref="StatusUpdate"/> event.
        /// </summary>
        /// <remarks>
        /// When this is true, the total number of emails in the source account will be counted before any emails are copied,
        /// which may take some time if there are many folders int he source account.
        /// </remarks>
        public bool ProgressUpdates { get; set; }

        /// <summary>
        /// When true, for each email message copied, an attempt will be made to see if it already exists on the destination server
        /// by trying to match the Message ID header.
        /// </summary>
        /// <remarks>
        /// If false, no checks will be made to see if the email message exists. This may lead to the duplication of
        /// emails in the destination account if multiple mgration attempts are made.
        /// </remarks>
        public bool ReplaceExisting { get; set; }

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
        /// <param name="creds">Mail account credentials to test.</param>
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
        /// Runs the configures migration job.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Awaitable task.</returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (SourceCredentials == null)
                throw new InvalidOperationException($"{nameof(SourceCredentials)} must be specified.");
            if (DestCredentials == null)
                throw new InvalidOperationException($"{nameof(DestCredentials)} must be specified.");

            totalMessages = 0;
            completedMessages = 0;
            progress = 0;
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

                    var personalNamespaces = srcHost.PersonalNamespaces;
                    var allFolders = await srcHost.GetFoldersAsync(personalNamespaces[0], false, cancellationToken);

                    DoStatus($"Found {allFolders.Count} folders on source server", false, StatusType.Info);

                    if (ProgressUpdates)
                    {
                        DoStatus($"Counting messages...", false, StatusType.Info);
                        await CountMesagesAsync(allFolders, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                            return;
                        DoStatus($"Found {totalMessages} messages on source server.", false, StatusType.Info);
                    }

                    DoStatus($"Connecting to destination: {DestCredentials.Server}...", false, StatusType.Info);
                    await ConnectHost(dstHost, DestCredentials, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    DoStatus("Connected to destination server", false, StatusType.Info);

                    var stats = new MigrationStats();

                    foreach (var srcFolder in allFolders)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        if (ShouldSkipFolder(srcFolder))
                        {
                            DoStatus($"Skipping special folder {srcFolder.FullName}.", false, StatusType.Info);
                            continue;
                        }

                        try
                        {
                            await MigrateFolderAsync(srcHost, dstHost, srcFolder, stats, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            DoStatus($"Error processing folder '{srcFolder.FullName}': {ex.Message}", false, StatusType.Error);
                        }
                    }

                    DoStatus("Move complete.", true, StatusType.Info);
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

        private int completedMessages;

        private bool deleteSource;

        private int progress;

        private FolderAccess srcAccess = FolderAccess.ReadOnly;

        private int totalMessages;

        private event StatusEvent? statusUpdate;
        private static async Task ConnectHost(ImapClient srcHost, Creds creds, CancellationToken cancellationToken)
        {
            await srcHost.ConnectAsync(creds.Server, 993, true, cancellationToken);

            if (creds.UseOAuth && !string.IsNullOrEmpty(creds.OAuthToken))
            {
                var oauth2 = new MailKit.Security.SaslMechanismOAuth2(creds.User, creds.OAuthToken);
                await srcHost.AuthenticateAsync(oauth2, cancellationToken);
            }
            else
            {
                await srcHost.AuthenticateAsync(creds.User, creds.Password, cancellationToken);
            }
        }

        private static IMailFolder? MatchSpecialFolder(IMailFolder folder,
            ImapClient client)
        {
            if (folder.Attributes.HasFlag(FolderAttributes.Inbox))
                return client.Inbox;

            foreach (var flag in Enum.GetValues<SpecialFolder>())
            {
                FolderAttributes attr = Enum.GetValues<FolderAttributes>()
                    .FirstOrDefault(a => a.ToString() == flag.ToString());
                if (attr != default
                    && folder.Attributes.HasFlag(attr))
                {
                    return client.GetFolder(flag);
                }
            }
            return null;
        }

        private async Task CountMesagesAsync(IList<IMailFolder> folders, CancellationToken cancellationToken)
        {
            totalMessages = 0;
            foreach (var folder in folders)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    if (!ShouldSkipFolder(folder))
                    {
                        await folder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
                        totalMessages += folder.Count;
                        await folder.CloseAsync(false, cancellationToken);
                        DoStatus($"{totalMessages} messages found - {folder.Count} discovered in {folder.FullName}", false, StatusType.Info);
                    }
                }
                catch (Exception e)
                {
                    DoStatus($"Error counting messages in source folder {folder.FullName}: {e.Message}.", false, StatusType.Error);
                }
            }
        }

        private void DoStatus(string message, bool progress, StatusType type)
        {
            bool send = false;
            if (progress)
            {
                if (totalMessages > 0)
                {
                    var prog = (int)((decimal)completedMessages / (decimal)totalMessages * 100m);
                    if (prog != this.progress)
                    {
                        send = true;
                        this.progress = prog;
                    }
                }

                send = send || completedMessages % 10 == 0;
            }
            else
            {
                send = true;
            }

            if (send)
            {
                OnProgressUpdate(new StatusEventArgs { Percentage = this.progress, Status = message, Type = type });
            }
        }

        private async Task<IMailFolder?> GetOrCreateFolderAsync(
            ImapClient client, IMailFolder srcFolder,
            CancellationToken cancellationToken)
        {
            if (srcFolder.Attributes.HasFlag(FolderAttributes.All))
            {
                DoStatus($"Using {srcFolder.FullName} to find archived messages.", false, StatusType.Info);
                return client.GetFolder(SpecialFolder.Archive);
            }

            var special = MatchSpecialFolder(srcFolder, client);
            if (special != null)
                return special;

            char replaceChar = client.Inbox.DirectorySeparator == '_'
                ? '-' : '_';
            List<string> parts = new List<string>();
            var srcRoot = srcFolder;
            parts.Add(srcRoot.Name.Replace(client.Inbox.DirectorySeparator, replaceChar));
            while (srcRoot.ParentFolder != null)
            {
                srcRoot = srcRoot.ParentFolder;
                special = MatchSpecialFolder(srcRoot, client);
                if (special != null)
                {
                    srcRoot = special;
                    break;
                }
                else if (!string.IsNullOrEmpty(srcRoot.Name))
                {
                    parts.Insert(0, srcRoot.Name);
                }
            }

            IMailFolder? dstRoot = null;

            special = MatchSpecialFolder(srcRoot, client);
            if (special != null)
            {
                dstRoot = special;
            }

            if (dstRoot == null)
            {
                dstRoot = client.Inbox.ParentFolder;
                if (dstRoot == null)
                    dstRoot = client.Inbox;
            }

            foreach (var part in parts)
            {
                try
                {
                    dstRoot = await dstRoot.GetSubfolderAsync(part, cancellationToken);
                }
                catch
                {
                    DoStatus($"Creating folder: {dstRoot.FullName}{dstRoot.DirectorySeparator}{part}", false, StatusType.Info);
                    dstRoot = await dstRoot.CreateAsync(part, true, cancellationToken);
                    await dstRoot.SubscribeAsync(cancellationToken);
                }
            }

            return dstRoot;
        }

        private async Task MigrateFolderAsync(
            ImapClient srcHost,
            ImapClient dstHost,
            IMailFolder srcFolder,
            MigrationStats stats,
            CancellationToken cancellationToken)
        {
            DoStatus($"Processing folder: {srcFolder.FullName}", false, StatusType.Info);

            try
            {
                await srcFolder.OpenAsync(srcAccess, cancellationToken);
            }
            catch (Exception ex)
            {
                DoStatus($"Cannot open folder (skipping): {ex.Message}", false, StatusType.Error);
                return;
            }

            if (srcFolder.Count == 0)
            {
                DoStatus($"Folder is empty, skipping", false, StatusType.Info);
                await srcFolder.CloseAsync(false, cancellationToken);
                return;
            }

            DoStatus($"Contains {srcFolder.Count} messages", false, StatusType.Info);

            var dstFolder = await GetOrCreateFolderAsync(dstHost, srcFolder, cancellationToken);
            if (dstFolder == null)
            {
                DoStatus($"Failed to create destination folder", false, StatusType.Error);
                await srcFolder.CloseAsync(false, cancellationToken);
                return;
            }

            if (FoldersOnly)
            {
                await srcFolder.CloseAsync(false, cancellationToken);
                return;
            }

            await dstFolder.OpenAsync(FolderAccess.ReadWrite, cancellationToken);
            bool checkDuplicate = dstFolder.Count > 0;

            var uids = srcFolder.Attributes.HasFlag(FolderAttributes.All)
                ? await srcFolder.SearchAsync(MailKit.Search.SearchQuery.GMailRawSearch("in:archive"), cancellationToken)
                : await srcFolder.SearchAsync(MailKit.Search.SearchQuery.All, cancellationToken);

            uids = uids.Reverse().ToList();
            string? curHdr = null;
            while (uids.Count > 0)
            {
                var uid = uids.First();
                stats.TotalMessages++;
                try
                {
                    var message = await srcFolder.GetMessageAsync(uid, cancellationToken);
                    bool skip = false;
                    var hdr = message.Headers.FirstOrDefault(
                        h => h.Id == MimeKit.HeaderId.MessageId);
                    if (hdr != null)
                    {
                        curHdr = hdr.Value;
                        if (checkDuplicate || ReplaceExisting)
                        {
                            var match = await dstFolder.SearchAsync(SearchQuery.HeaderContains(
                                hdr.Field, hdr.Value),
                                cancellationToken);
                            if (ReplaceExisting)
                            {
                                foreach (var delId in match)
                                {
                                    await dstFolder.AddFlagsAsync(uid, MessageFlags.Deleted, true, cancellationToken);
                                }
                            }
                            else
                            {
                                skip = match.Count > 0;
                            }
                        }
                    }
                    else
                    {
                        curHdr = null;
                    }

                    if (!skip)
                    {
                        var items = await srcFolder.FetchAsync(new[] { uid }, MessageSummaryItems.Flags, cancellationToken);
                        var flags = items.FirstOrDefault()?.Flags ?? MessageFlags.None;

                        await dstFolder.AppendAsync(message, flags, message.Date, cancellationToken);
                        if (DeleteSource)
                            await srcFolder.AddFlagsAsync(uid, MessageFlags.Deleted, true, cancellationToken);

                        stats.SuccessfulMessages++;
                    }
                    else
                    {
                        stats.SkippedMessages++;
                    }
                    uids.RemoveAt(0);
                    completedMessages++;
                    DoStatus($"Migrated {completedMessages} messages", true, StatusType.Info);
                }
                catch (Exception ex)
                {
                    if (!srcHost.IsConnected || !dstHost.IsConnected)
                    {
                        try
                        {
                            if (!srcHost.IsConnected)
                            {
                                DoStatus($"Source is disconnected, reconnecting...", false, StatusType.Warning);
                                await ConnectHost(srcHost, SourceCredentials, cancellationToken);
                                srcFolder = await srcHost.GetFolderAsync(srcFolder.FullName, cancellationToken);
                                await srcFolder.OpenAsync(srcAccess, cancellationToken);
                                DoStatus($"Reconnected.", false, StatusType.Warning);
                            }
                            if (!dstHost.IsConnected)
                            {
                                DoStatus($"Destination is disconnected, reconnecting...", false, StatusType.Warning);
                                await ConnectHost(dstHost, DestCredentials, cancellationToken);
                                dstFolder = await dstHost.GetFolderAsync(dstFolder.FullName, cancellationToken);
                                await dstFolder.OpenAsync(FolderAccess.ReadWrite, cancellationToken);
                                DoStatus($"Reconnected.", false, StatusType.Warning);
                            }
                            stats.TotalMessages--;
                        }
                        catch (Exception e)
                        {
                            DoStatus($"Error while reconnection. Aborting.", false, StatusType.Error);
                            return;
                        }
                    }
                    else
                    {
                        stats.FailedMessages++;
                        DoStatus($"Failed to migrate message with header {curHdr}: {ex.Message}", false, StatusType.Warning);
                    }
                }
            }

            try
            {
                if (DeleteSource)
                {
                    await srcFolder.ExpungeAsync(cancellationToken);
                    DoStatus($"Expunged deleted messages from source", false, StatusType.Info);
                }
                if (ReplaceExisting)
                {
                    await dstFolder.ExpungeAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                DoStatus($"Warning: Failed to expunge folder: {ex.Message}", false, StatusType.Info);
            }

            await dstFolder.CloseAsync(false, cancellationToken);
            await srcFolder.CloseAsync(false, cancellationToken);

            DoStatus($"Completed folder: {srcFolder.FullName}", false, StatusType.Info);
        }

        private void SetSourceAccess()
        {
            if (deleteSource)
                srcAccess = FolderAccess.ReadWrite;
            else
                srcAccess = FolderAccess.ReadWrite;
        }

        private bool ShouldSkipFolder(IMailFolder folder)
        {
            if ((folder.Attributes & (FolderAttributes.Flagged | FolderAttributes.Important)) != FolderAttributes.None)
            {
                return true;
            }
            if (folder.FullName == "[Gmail]/Important" || folder.FullName == "[Gmail]")
            {
                return true;
            }
            return false;
        }
    }
}