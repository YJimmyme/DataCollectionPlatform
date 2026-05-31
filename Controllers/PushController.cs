using System.Text.Json;
using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;
using DataCollectionPlatform.Models.ViewModels;
using DataCollectionPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Controllers;

public class PushController(AppDbContext db, IPushExecutor pushExecutor) : Controller
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<IActionResult> Index()
    {
        var schedules = await db.PushSchedules
            .Include(s => s.Logs)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        return View(schedules);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormVm(new PushFormViewModel()));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PushFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(await BuildFormVm(vm));

        db.PushSchedules.Add(new PushSchedule
        {
            Name         = vm.Name,
            Platform     = vm.Platform,
            IntervalType = vm.IntervalType,
            RunAt        = ParseTime(vm.RunAtTime),
            DayOfWeek    = vm.IntervalType == "weekly"  ? vm.DayOfWeek  : null,
            DayOfMonth   = vm.IntervalType == "monthly" ? vm.DayOfMonth : null,
            NewItemsOnly = vm.NewItemsOnly,
            IsActive     = vm.IsActive,
            FilterJson   = SerializeFilter(vm),
            CreatedAt    = DateTime.Now
        });
        await db.SaveChangesAsync();
        TempData["Success"] = $"排程「{vm.Name}」已建立";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var s = await db.PushSchedules.FindAsync(id);
        if (s == null) return NotFound();

        var filter = JsonSerializer.Deserialize<PushScheduleFilter>(
            s.FilterJson ?? "{}", JsonOpts) ?? new();

        var vm = new PushFormViewModel
        {
            Id                  = s.Id,
            Name                = s.Name,
            Platform            = s.Platform,
            IntervalType        = s.IntervalType,
            RunAtTime           = s.RunAt.ToString(@"hh\:mm"),
            DayOfWeek           = s.DayOfWeek ?? 1,
            DayOfMonth          = s.DayOfMonth ?? 1,
            NewItemsOnly        = s.NewItemsOnly,
            IsActive            = s.IsActive,
            Query               = filter.Query,
            SelectedDataTypeIds = filter.DataTypeIds,
            SelectedTopicIds    = filter.TopicIds
        };
        return View(await BuildFormVm(vm));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PushFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid)
            return View(await BuildFormVm(vm));

        var s = await db.PushSchedules.FindAsync(id);
        if (s == null) return NotFound();

        s.Name         = vm.Name;
        s.Platform     = vm.Platform;
        s.IntervalType = vm.IntervalType;
        s.RunAt        = ParseTime(vm.RunAtTime);
        s.DayOfWeek    = vm.IntervalType == "weekly"  ? vm.DayOfWeek  : null;
        s.DayOfMonth   = vm.IntervalType == "monthly" ? vm.DayOfMonth : null;
        s.NewItemsOnly = vm.NewItemsOnly;
        s.IsActive     = vm.IsActive;
        s.FilterJson   = SerializeFilter(vm);

        await db.SaveChangesAsync();
        TempData["Success"] = "排程已更新";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await db.PushSchedules.FindAsync(id);
        if (s != null) { db.PushSchedules.Remove(s); await db.SaveChangesAsync(); }
        TempData["Success"] = "排程已刪除";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var s = await db.PushSchedules.FindAsync(id);
        if (s == null) return NotFound();
        s.IsActive = !s.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(int id)
    {
        var s = await db.PushSchedules.Include(x => x.Logs).FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();

        var log = await pushExecutor.ExecuteAsync(s);
        TempData[log.Status == "success" ? "Success" : "Error"] =
            $"【{s.Name}】{log.Message}";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Log(int id)
    {
        var s = await db.PushSchedules
            .Include(x => x.Logs.OrderByDescending(l => l.RunAt))
            .FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();
        return View(s);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<PushFormViewModel> BuildFormVm(PushFormViewModel vm)
    {
        vm.AllDataTypes = await db.DataTypes.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync();
        var all = await db.Topics.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
        vm.TopicTree = Flatten(all, null, 0);
        return vm;
    }

    private static string SerializeFilter(PushFormViewModel vm) =>
        JsonSerializer.Serialize(new PushScheduleFilter
        {
            Query       = vm.Query,
            DataTypeIds = vm.SelectedDataTypeIds,
            TopicIds    = vm.SelectedTopicIds
        });

    private static TimeSpan ParseTime(string t) =>
        TimeSpan.TryParse(t, out var ts) ? ts : TimeSpan.FromHours(9);

    private static List<TopicTreeNode> Flatten(List<Topic> all, int? parentId, int level)
    {
        var result = new List<TopicTreeNode>();
        foreach (var t in all.Where(t => t.ParentId == parentId))
        {
            result.Add(new TopicTreeNode { Id = t.Id, Name = t.Name, Level = level });
            result.AddRange(Flatten(all, t.Id, level + 1));
        }
        return result;
    }
}
