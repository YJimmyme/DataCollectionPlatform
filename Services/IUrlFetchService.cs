namespace DataCollectionPlatform.Services;

public interface IUrlFetchService
{
    Task<UrlMetadata?> FetchAsync(string url);
}

public record UrlMetadata(string? Title, string? Description, string? Author);
