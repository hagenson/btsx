namespace Btsx
{
    public interface IJob
    {
        string Id { get; set; }
        DateTime StartTime { get; set; }
        bool IsCompleted { get; set; }
        string Status { get; set; }
        int Progress { get; set; }
        string StatusType { get; set; }
        DateTime? EndTime { get; set; }
        MigrationStats? Statistics { get; set; }
        MigrationRequest Request { get; set; }
    }
}