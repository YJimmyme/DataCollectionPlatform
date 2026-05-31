namespace DataCollectionPlatform.Models;

public class PushLog
{
    public int Id { get; set; }
    public int PushScheduleId { get; set; }
    public PushSchedule PushSchedule { get; set; } = null!;
    public DateTime RunAt { get; set; } = DateTime.Now;
    public string Status { get; set; } = "";  // success | failed
    public int ItemCount { get; set; }
    public string? Message { get; set; }
}
