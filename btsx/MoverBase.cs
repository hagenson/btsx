namespace Btsx
{
    /// <summary>
    /// Provides a common base for <see cref="IMover"/> implementations.
    /// </summary>
    public abstract class MoverBase : IMover
    {
        /// <inheritdoc/>
        public event StatusEvent? StatusUpdate
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

        /// <inheritdoc/>
        public bool ProgressUpdates { get; set; }

        /// <inheritdoc/>
        public MigrationStats? Statistics { get; protected set; }

        /// <inheritdoc/>
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);

        /// <inheritdoc/>
        public abstract Task<bool> TestAuthenticationAsync(Creds creds, CancellationToken cancellationToken);

        /// <summary>
        /// Count of completed items.
        /// </summary>
        protected int completedItems;

        /// <summary>
        /// Percentage complete.
        /// </summary>
        protected int progress;

        /// <summary>
        /// Count of total items to process.
        /// </summary>
        protected int totalItems;

        /// <summary>
        /// Helper method to trigger a <see cref="StatusUpdate"/> event in a standard way.
        /// </summary>
        /// <param name="message">Display message.</param>
        /// <param name="progress">If true, the event will only be triggered if the <see cref="progress"/>
        /// value has changed since the last call to this method.</param>
        /// <param name="type">Type of notification to send.</param>
        protected void DoStatus(string message, bool progress, StatusType type)
        {
            bool send = false;
            var prog = totalItems > 0
                ? (int)((decimal)completedItems / (decimal)totalItems * 100m)
                : 0;
            if (progress)
            {
                if (totalItems > 0)
                {
                    if (prog != this.progress)
                    {
                        send = true;
                        this.progress = prog;
                    }
                }

                send = send || completedItems % 10 == 0;
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

        /// <summary>
        /// Called to notify status updates.
        /// </summary>
        /// <param name="args">Migration job status.</param>
        protected virtual void OnProgressUpdate(StatusEventArgs args)
        {
            statusUpdate?.Invoke(this, args);
        }

        private event StatusEvent? statusUpdate;
    }
}