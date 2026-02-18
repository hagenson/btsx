namespace Btsx.Persistence
{
    internal class PersistedJob : IJob
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set;  }
        public bool IsCompleted { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
        public string StatusType { get; set; }
        public DateTime? EndTime { get; set; }
        public MigrationStats? Statistics { get; set; }
        public MigrationRequest Request { get; set; }
    }
}
