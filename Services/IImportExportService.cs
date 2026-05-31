using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;

namespace DataCollectionPlatform.Services;

public interface IImportExportService
{
    Task<byte[]> ExportCsvAsync(IEnumerable<Item> items);
    Task<ImportResult> ImportCsvAsync(Stream stream, AppDbContext db);
}

public record ImportResult(int Success, int Failed, string Errors);
