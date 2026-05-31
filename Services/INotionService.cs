using DataCollectionPlatform.Models;

namespace DataCollectionPlatform.Services;

public interface INotionService
{
    Task<(bool Success, string Message)> PushItemsAsync(IEnumerable<Item> items);
}
