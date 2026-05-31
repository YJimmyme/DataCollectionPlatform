using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;
using DataCollectionPlatform.Models.ViewModels;
using DataCollectionPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Controllers;

public class ItemsController(AppDbContext db, IImportExportService exportService) : Controller
{
    public async Task<IActionResult> Index(ItemIndexViewModel filter)
    {
        if (filter.Page < 1) filter.Page = 1;

        var query = db.Items
            .Include(x => x.ItemDataTypes).ThenInclude(x => x.DataType)
            .Include(x => x.ItemTopics).ThenInclude(x => x.Topic)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var q = filter.Query.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(q) ||
                (x.Summary != null && x.Summary.ToLower().Contains(q)) ||
                (x.Tags != null && x.Tags.ToLower().Contains(q)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(q)));
        }

        if (filter.SelectedDataTypeIds.Count > 0)
            query = query.Where(x =>
                x.ItemDataTypes.Any(d => filter.SelectedDataTypeIds.Contains(d.DataTypeId)));

        if (filter.SelectedTopicIds.Count > 0)
            query = query.Where(x =>
                x.ItemTopics.Any(t => filter.SelectedTopicIds.Contains(t.TopicId)));

        if (filter.DateFrom.HasValue)
            query = query.Where(x => x.PublishedAt >= filter.DateFrom);

        if (filter.DateTo.HasValue)
            query = query.Where(x => x.PublishedAt <= filter.DateTo);

        query = filter.SortBy switch
        {
            "title" => query.OrderBy(x => x.Title),
            "published_desc" => query.OrderByDescending(x => x.PublishedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        filter.TotalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        filter.Items = items.Select(x => new ItemListRow
        {
            Id = x.Id,
            Title = x.Title,
            SourceUrl = x.SourceUrl,
            SourceRef = x.SourceRef,
            Author = x.Author,
            PublishedAt = x.PublishedAt,
            CreatedAt = x.CreatedAt,
            DataTypeNames = x.ItemDataTypes.Select(d => d.DataType.Name).ToList(),
            TopicNames = x.ItemTopics.Select(t => t.Topic.Name).ToList(),
            TagList = string.IsNullOrWhiteSpace(x.Tags)
                ? []
                : [.. x.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)]
        }).ToList();

        filter.AllDataTypes = await db.DataTypes.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync();
        filter.TopicRoots = await BuildTopicRoots();

        return View(filter);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var item = await db.Items
            .Include(x => x.ItemDataTypes).ThenInclude(x => x.DataType)
            .Include(x => x.ItemTopics).ThenInclude(x => x.Topic)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null) return NotFound();

        return View(new ItemDetailViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Summary = item.Summary,
            SourceUrl = item.SourceUrl,
            SourceRef = item.SourceRef,
            SourcePage = item.SourcePage,
            Author = item.Author,
            PublishedAt = item.PublishedAt,
            Tags = item.Tags,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            DataTypeNames = item.ItemDataTypes.Select(d => d.DataType.Name).ToList(),
            TopicNames = item.ItemTopics.Select(t => t.Topic.Name).ToList()
        });
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormViewModel(new ItemFormViewModel()));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemFormViewModel vm)
    {
        ValidateSourceAndSelection(vm);
        if (!ModelState.IsValid)
            return View(await BuildFormViewModel(vm));

        var item = MapToItem(vm);
        db.Items.Add(item);
        await db.SaveChangesAsync();
        await SaveRelations(item.Id, vm);

        TempData["Success"] = "資料已新增";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.Items
            .Include(x => x.ItemDataTypes)
            .Include(x => x.ItemTopics)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null) return NotFound();

        var vm = new ItemFormViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Summary = item.Summary,
            SourceUrl = item.SourceUrl,
            SourceRef = item.SourceRef,
            SourcePage = item.SourcePage,
            Author = item.Author,
            PublishedAt = item.PublishedAt,
            Tags = item.Tags,
            Notes = item.Notes,
            SelectedDataTypeIds = item.ItemDataTypes.Select(d => d.DataTypeId).ToList(),
            SelectedTopicIds = item.ItemTopics.Select(t => t.TopicId).ToList()
        };

        return View(await BuildFormViewModel(vm));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        ValidateSourceAndSelection(vm);
        if (!ModelState.IsValid)
            return View(await BuildFormViewModel(vm));

        var item = await db.Items
            .Include(x => x.ItemDataTypes)
            .Include(x => x.ItemTopics)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null) return NotFound();

        item.Title = vm.Title;
        item.Summary = vm.Summary;
        item.SourceUrl = Nz(vm.SourceUrl);
        item.SourceRef = Nz(vm.SourceRef);
        item.SourcePage = Nz(vm.SourcePage);
        item.Author = Nz(vm.Author);
        item.PublishedAt = vm.PublishedAt;
        item.Tags = Nz(vm.Tags);
        item.Notes = Nz(vm.Notes);
        item.UpdatedAt = DateTime.Now;

        db.ItemDataTypes.RemoveRange(item.ItemDataTypes);
        db.ItemTopics.RemoveRange(item.ItemTopics);
        await db.SaveChangesAsync();
        await SaveRelations(id, vm);

        TempData["Success"] = "資料已更新";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Items.FindAsync(id);
        if (item != null)
        {
            db.Items.Remove(item);
            await db.SaveChangesAsync();
        }
        TempData["Success"] = "資料已刪除";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(ItemIndexViewModel filter)
    {
        var query = db.Items
            .Include(x => x.ItemDataTypes).ThenInclude(x => x.DataType)
            .Include(x => x.ItemTopics).ThenInclude(x => x.Topic)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var q = filter.Query.ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(q) ||
                (x.Summary != null && x.Summary.ToLower().Contains(q)));
        }
        if (filter.SelectedDataTypeIds.Count > 0)
            query = query.Where(x =>
                x.ItemDataTypes.Any(d => filter.SelectedDataTypeIds.Contains(d.DataTypeId)));
        if (filter.SelectedTopicIds.Count > 0)
            query = query.Where(x =>
                x.ItemTopics.Any(t => filter.SelectedTopicIds.Contains(t.TopicId)));

        var items = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        var bytes = await exportService.ExportCsvAsync(items);
        var filename = $"items_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv; charset=utf-8", filename);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "請選擇 CSV 檔案";
            return RedirectToAction(nameof(Index));
        }

        using var stream = file.OpenReadStream();
        var result = await exportService.ImportCsvAsync(stream, db);

        TempData["Success"] = $"匯入完成：成功 {result.Success} 筆，失敗 {result.Failed} 筆";
        if (!string.IsNullOrWhiteSpace(result.Errors))
            TempData["Error"] = result.Errors;

        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void ValidateSourceAndSelection(ItemFormViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.SourceUrl) && string.IsNullOrWhiteSpace(vm.SourceRef))
            ModelState.AddModelError("SourceUrl", "來源網址或出處說明至少填寫一項");

        if (vm.SelectedDataTypeIds.Count == 0)
            ModelState.AddModelError("SelectedDataTypeIds", "至少選擇一個資料類別");

        if (vm.SelectedTopicIds.Count == 0)
            ModelState.AddModelError("SelectedTopicIds", "至少選擇一個資訊種類");
    }

    private static Item MapToItem(ItemFormViewModel vm) => new()
    {
        Title = vm.Title,
        Summary = vm.Summary,
        SourceUrl = Nz(vm.SourceUrl),
        SourceRef = Nz(vm.SourceRef),
        SourcePage = Nz(vm.SourcePage),
        Author = Nz(vm.Author),
        PublishedAt = vm.PublishedAt,
        Tags = Nz(vm.Tags),
        Notes = Nz(vm.Notes),
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now
    };

    private async Task SaveRelations(int itemId, ItemFormViewModel vm)
    {
        foreach (var dtId in vm.SelectedDataTypeIds)
            db.ItemDataTypes.Add(new ItemDataType { ItemId = itemId, DataTypeId = dtId });
        foreach (var topicId in vm.SelectedTopicIds)
            db.ItemTopics.Add(new ItemTopic { ItemId = itemId, TopicId = topicId });
        await db.SaveChangesAsync();
    }

    private async Task<ItemFormViewModel> BuildFormViewModel(ItemFormViewModel vm)
    {
        vm.AllDataTypes = await db.DataTypes.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync();
        var allTopics = await db.Topics.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
        vm.TopicTree = FlattenTopics(allTopics, null, 0);
        return vm;
    }

    private static List<TopicTreeNode> FlattenTopics(List<Models.Topic> all, int? parentId, int level)
    {
        var result = new List<TopicTreeNode>();
        foreach (var t in all.Where(t => t.ParentId == parentId))
        {
            result.Add(new TopicTreeNode { Id = t.Id, Name = t.Name, Level = level });
            result.AddRange(FlattenTopics(all, t.Id, level + 1));
        }
        return result;
    }

    private async Task<List<Models.Topic>> BuildTopicRoots()
    {
        var all = await db.Topics.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
        var roots = all.Where(t => t.ParentId == null).ToList();
        foreach (var root in roots)
            root.Children = all.Where(t => t.ParentId == root.Id).ToList();
        return roots;
    }

    private static string? Nz(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
