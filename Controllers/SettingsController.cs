using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;
using DataCollectionPlatform.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Controllers;

public class SettingsController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string tab = "datatypes")
    {
        var allTopics = await db.Topics.OrderBy(t => t.Name).ToListAsync();
        var vm = new SettingsViewModel
        {
            ActiveTab = tab,
            DataTypes = await db.DataTypes.OrderBy(d => d.Name).ToListAsync(),
            ActiveDataTypes = await db.DataTypes.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(),
            Topics = FlattenTopics(allTopics, null, 0)
        };
        return View(vm);
    }

    // ── DataType ──────────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDataType(string name)
    {
        name = name?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "名稱不可為空";
            return RedirectToAction(nameof(Index), new { tab = "datatypes" });
        }

        if (await db.DataTypes.AnyAsync(d => d.Name == name))
        {
            TempData["Error"] = $"「{name}」已存在";
            return RedirectToAction(nameof(Index), new { tab = "datatypes" });
        }

        db.DataTypes.Add(new DataType { Name = name });
        await db.SaveChangesAsync();
        TempData["Success"] = $"已新增類別「{name}」";
        return RedirectToAction(nameof(Index), new { tab = "datatypes" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDataType(int id, string name)
    {
        var dt = await db.DataTypes.FindAsync(id);
        if (dt == null) return NotFound();

        name = name?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "名稱不可為空";
            return RedirectToAction(nameof(Index), new { tab = "datatypes" });
        }

        dt.Name = name;
        await db.SaveChangesAsync();
        TempData["Success"] = "類別名稱已更新";
        return RedirectToAction(nameof(Index), new { tab = "datatypes" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDataType(int id)
    {
        var dt = await db.DataTypes.FindAsync(id);
        if (dt == null) return NotFound();

        dt.IsActive = !dt.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { tab = "datatypes" });
    }

    // ── Topic ──────────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTopic(string name, int? parentId)
    {
        name = name?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "名稱不可為空";
            return RedirectToAction(nameof(Index), new { tab = "topics" });
        }

        db.Topics.Add(new Topic { Name = name, ParentId = parentId == 0 ? null : parentId });
        await db.SaveChangesAsync();
        TempData["Success"] = $"已新增主題「{name}」";
        return RedirectToAction(nameof(Index), new { tab = "topics" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTopic(int id, string name)
    {
        var topic = await db.Topics.FindAsync(id);
        if (topic == null) return NotFound();

        name = name?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "名稱不可為空";
            return RedirectToAction(nameof(Index), new { tab = "topics" });
        }

        topic.Name = name;
        await db.SaveChangesAsync();
        TempData["Success"] = "主題名稱已更新";
        return RedirectToAction(nameof(Index), new { tab = "topics" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTopic(int id)
    {
        var topic = await db.Topics.FindAsync(id);
        if (topic == null) return NotFound();

        topic.IsActive = !topic.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { tab = "topics" });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<TopicFlatRow> FlattenTopics(List<Topic> all, int? parentId, int level)
    {
        var result = new List<TopicFlatRow>();
        foreach (var t in all.Where(t => t.ParentId == parentId))
        {
            var parentName = all.FirstOrDefault(p => p.Id == t.ParentId)?.Name ?? "";
            result.Add(new TopicFlatRow
            {
                Id = t.Id, Name = t.Name, ParentId = t.ParentId,
                ParentName = parentName, IsActive = t.IsActive, Level = level
            });
            result.AddRange(FlattenTopics(all, t.Id, level + 1));
        }
        return result;
    }
}
