namespace Btsx
{
    public abstract class MoverBase : IMover
    {
        /// <summary>
        /// Event triggered to report status updates.
        /// </summary>
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

        public bool ProgressUpdates { get; set; }
        public MigrationStats? Statistics { get; protected set; }
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
        public abstract Task<bool> TestAuthenticationAsync(Creds creds, CancellationToken cancellationToken);

        protected int completedItems;

        protected int progress;

        protected int totalItems;
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