using System.Text.Json;
using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Services;

public class PushExecutor(
    AppDbContext db,
    INotionService notionService,
    IEmailService emailService) : IPushExecutor
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PushLog> ExecuteAsync(PushSchedule schedule)
    {
        var log = new PushLog
        {
            PushScheduleId = schedule.Id,
            RunAt          = DateTime.Now,
            Status         = "running"
        };
        db.PushLogs.Add(log);
        await db.SaveChangesAsync();

        try
        {
            var items = await QueryItemsAsync(schedule);

            var (success, message) = schedule.Platform switch
            {
                "notion" => await notionService.PushItemsAsync(items),
                "email"  => await emailService.SendItemsAsync(items, schedule.Name),
                _        => (false, $"未知平台：{schedule.Platform}")
            };

            log.Status    = success ? "success" : "failed";
            log.ItemCount = items.Count;
            log.Message   = message;
        }
        catch (Exception ex)
        {
            log.Status  = "failed";
            log.Message = ex.Message;
        }

        schedule.LastRunAt = DateTime.Now;
        await db.SaveChangesAsync();
        return log;
    }

    private async Task<List<Item>> QueryItemsAsync(PushSchedule schedule)
    {
        var filter = JsonSerializer.Deserialize<PushScheduleFilter>(
            schedule.FilterJson ?? "{}", JsonOpts) ?? new();

        var query = db.Items
            .Include(x => x.ItemDataTypes).ThenInclude(x => x.DataType)
            .Include(x => x.ItemTopics).ThenInclude(x => x.Topic)
            .AsQueryable();

        if (schedule.NewItemsOnly && schedule.LastRunAt.HasValue)
            query = query.Where(x => x.CreatedAt >= schedule.LastRunAt.Value);

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var q = filter.Query.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(q) ||
                (x.Summary != null && x.Summary.ToLower().Contains(q)));
        }

        if (filter.DataTypeIds.Count > 0)
            query = query.Where(x =>
                x.ItemDataTypes.Any(d => filter.DataTypeIds.Contains(d.DataTypeId)));

        if (filter.TopicIds.Count > 0)
            query = query.Where(x =>
                x.ItemTopics.Any(t => filter.TopicIds.Contains(t.TopicId)));

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }
}
