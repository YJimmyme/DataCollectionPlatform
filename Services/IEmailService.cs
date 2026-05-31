using DataCollectionPlatform.Models;

namespace DataCollectionPlatform.Services;

public interface IEmailService
{
    Task<(bool Success, string Message)> SendItemsAsync(IEnumerable<Item> items, string scheduleName);
}
