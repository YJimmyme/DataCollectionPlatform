namespace DataCollectionPlatform.Models;

public class PushSchedule
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Platform { get; set; } = "email";       // email | notion
    public string FilterJson { get; set; } = "{}";
    public string IntervalType { get; set; } = "manual";  // manual | daily | weekly | monthly
    public TimeSpan RunAt { get; set; } = TimeSpan.FromHours(9);
    public int? DayOfWeek { get; set; }   // 0=Sun … 6=Sat（weekly 用）
    public int? DayOfMonth { get; set; }  // 1-31（monthly 用）
    public bool NewItemsOnly { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<PushLog> Logs { get; set; } = [];
}

public class PushScheduleFilter
{
    public string? Query { get; set; }
    public List<int> DataTypeIds { get; set; } = [];
    public List<int> TopicIds { get; set; } = [];
}
