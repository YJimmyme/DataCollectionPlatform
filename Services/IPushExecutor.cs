using DataCollectionPlatform.Models;

namespace DataCollectionPlatform.Services;

public interface IPushExecutor
{
    Task<PushLog> ExecuteAsync(PushSchedule schedule);
}
