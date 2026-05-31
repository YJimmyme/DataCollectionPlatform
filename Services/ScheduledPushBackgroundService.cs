using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Services;

public class ScheduledPushBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ScheduledPushBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRunAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "排程推送背景服務發生例外");
            }

            // 等到下一分鐘整點再次檢查
            var now  = DateTime.Now;
            var next = now.AddMinutes(1)
                          .AddSeconds(-now.Second)
                          .AddMilliseconds(-now.Millisecond);
            await Task.Delay(next - now, stoppingToken);
        }
    }

    private async Task CheckAndRunAsync()
    {
        using var scope    = scopeFactory.CreateScope();
        var db             = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var executor       = scope.ServiceProvider.GetRequiredService<IPushExecutor>();

        var now       = DateTime.Now;
        var schedules = await db.PushSchedules
            .Where(s => s.IsActive && s.IntervalType != "manual")
            .ToListAsync();

        foreach (var s in schedules)
        {
            if (ShouldRun(s, now))
            {
                logger.LogInformation("執行排程推送：{Name}", s.Name);
                await executor.ExecuteAsync(s);
            }
        }
    }

    private static bool ShouldRun(PushSchedule s, DateTime now)
    {
        // 必須匹配設定的執行時間（精確到分鐘）
        if (now.Hour != s.RunAt.Hours || now.Minute != s.RunAt.Minutes)
            return false;

        // 若今天已執行過，跳過
        if (s.LastRunAt.HasValue)
        {
            var last = s.LastRunAt.Value;
            if (s.IntervalType == "daily"   && last.Date == now.Date) return false;
            if (s.IntervalType == "weekly"  && (now - last).TotalDays < 7) return false;
            if (s.IntervalType == "monthly" && last.Year == now.Year && last.Month == now.Month) return false;
        }

        return s.IntervalType switch
        {
            "daily"   => true,
            "weekly"  => s.DayOfWeek.HasValue && (int)now.DayOfWeek == s.DayOfWeek.Value,
            "monthly" => s.DayOfMonth.HasValue && now.Day == s.DayOfMonth.Value,
            _         => false
        };
    }
}
