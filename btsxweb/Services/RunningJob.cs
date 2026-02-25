using Btsx;

namespace BtsxWeb.Services
{
    internal class RunningJob
    {
        public RunningJob(IJob job)
        {
            CancellationTokenSource = new CancellationTokenSource();
            Job = job;
        }

        public CancellationTokenSource CancellationTokenSource { get; }

        public IJob Job { get; }
    }
}