using HtmlAgilityPack;

namespace DataCollectionPlatform.Services;

public class UrlFetchService(HttpClient http) : IUrlFetchService
{
    public async Task<UrlMetadata?> FetchAsync(string url)
    {
        try
        {
            http.DefaultRequestHeaders.UserAgent.TryParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var html = await http.GetStringAsync(url, cts.Token);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            string? Meta(string selector) =>
                doc.DocumentNode.SelectSingleNode(selector)
                   ?.GetAttributeValue("content", null)?.Trim();

            var title = Meta("//meta[@property='og:title']")
                     ?? doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();

            var description = Meta("//meta[@property='og:description']")
                           ?? Meta("//meta[@name='description']");

            var author = Meta("//meta[@name='author']");

            return new UrlMetadata(title, description, author);
        }
        catch
        {
            return null;
        }
    }
}
