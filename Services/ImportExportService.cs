using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Services;

public class ImportExportService : IImportExportService
{
    public async Task<byte[]> ExportCsvAsync(IEnumerable<Item> items)
    {
        using var ms = new MemoryStream();
        await using var writer = new StreamWriter(ms, new UTF8Encoding(true));
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader<ItemCsvRecord>();
        await csv.NextRecordAsync();

        foreach (var item in items)
        {
            csv.WriteRecord(new ItemCsvRecord
            {
                Title = item.Title,
                Summary = item.Summary ?? "",
                SourceUrl = item.SourceUrl ?? "",
                SourceRef = item.SourceRef ?? "",
                SourcePage = item.SourcePage ?? "",
                Author = item.Author ?? "",
                PublishedAt = item.PublishedAt?.ToString("yyyy-MM-dd") ?? "",
                Tags = item.Tags ?? "",
                Notes = item.Notes ?? "",
                DataTypes = string.Join(";", item.ItemDataTypes.Select(d => d.DataType?.Name ?? "")),
                Topics = string.Join(";", item.ItemTopics.Select(t => t.Topic?.Name ?? ""))
            });
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync();
        return ms.ToArray();
    }

    public async Task<ImportResult> ImportCsvAsync(Stream stream, AppDbContext db)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);

        var allDataTypes = await db.DataTypes.ToListAsync();
        var allTopics = await db.Topics.ToListAsync();

        int success = 0, failed = 0;
        var errors = new List<string>();

        await foreach (var row in csv.GetRecordsAsync<ItemCsvRecord>())
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.Title))
                    throw new Exception("標題為必填");
                if (string.IsNullOrWhiteSpace(row.SourceUrl) && string.IsNullOrWhiteSpace(row.SourceRef))
                    throw new Exception("來源網址或出處說明至少填寫一項");

                var item = new Item
                {
                    Title = row.Title.Trim(),
                    Summary = Nz(row.Summary),
                    SourceUrl = Nz(row.SourceUrl),
                    SourceRef = Nz(row.SourceRef),
                    SourcePage = Nz(row.SourcePage),
                    Author = Nz(row.Author),
                    Tags = Nz(row.Tags),
                    Notes = Nz(row.Notes),
                    PublishedAt = DateTime.TryParse(row.PublishedAt, out var dt) ? dt : null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.Items.Add(item);
                await db.SaveChangesAsync();

                foreach (var name in Split(row.DataTypes))
                {
                    var dt2 = allDataTypes.FirstOrDefault(d => d.Name == name);
                    if (dt2 != null)
                        db.ItemDataTypes.Add(new ItemDataType { ItemId = item.Id, DataTypeId = dt2.Id });
                }
                foreach (var name in Split(row.Topics))
                {
                    var t = allTopics.FirstOrDefault(x => x.Name == name);
                    if (t != null)
                        db.ItemTopics.Add(new ItemTopic { ItemId = item.Id, TopicId = t.Id });
                }
                await db.SaveChangesAsync();
                success++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"第 {success + failed} 筆：{ex.Message}");
            }
        }

        return new ImportResult(success, failed, string.Join("\n", errors));
    }

    private static string? Nz(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static IEnumerable<string> Split(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? []
            : s.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public class ItemCsvRecord
{
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
    public string? SourceUrl { get; set; }
    public string? SourceRef { get; set; }
    public string? SourcePage { get; set; }
    public string? Author { get; set; }
    public string? PublishedAt { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public string? DataTypes { get; set; }
    public string? Topics { get; set; }
}
