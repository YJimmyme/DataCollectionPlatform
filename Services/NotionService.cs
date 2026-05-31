using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Services;

public class NotionService(IHttpClientFactory httpClientFactory, AppDbContext db) : INotionService
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<(bool Success, string Message)> PushItemsAsync(IEnumerable<Item> items)
    {
        var settings = await db.AppSettings
            .Where(s => s.Key.StartsWith("notion."))
            .ToDictionaryAsync(s => s.Key, s => s.Value ?? "");

        var token = settings.GetValueOrDefault("notion.api_token", "");
        var dbId  = settings.GetValueOrDefault("notion.database_id", "");

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(dbId))
            return (false, "Notion API Token 或 Database ID 尚未設定，請至「設定 → 推送平台」填入");

        var client = httpClientFactory.CreateClient("notion");
        client.BaseAddress = new Uri("https://api.notion.com/");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");

        int ok = 0;
        var errors = new List<string>();

        foreach (var item in items)
        {
            var payload = BuildPagePayload(item, dbId);
            var json = JsonSerializer.Serialize(payload, JsonOpts);
            var response = await client.PostAsync("v1/pages",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                ok++;
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                errors.Add($"「{item.Title}」: {TruncateError(body)}");
            }
        }

        var msg = $"成功推送 {ok} 筆至 Notion";
        if (errors.Count > 0)
            msg += $"，失敗 {errors.Count} 筆：{string.Join("；", errors.Take(3))}";

        return (errors.Count == 0, msg);
    }

    private static object BuildPagePayload(Item item, string databaseId)
    {
        var multiSelect = (IEnumerable<string> vals) =>
            vals.Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => new { name = v.Trim() })
                .ToArray();

        var tags = string.IsNullOrWhiteSpace(item.Tags)
            ? []
            : item.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var properties = new Dictionary<string, object>
        {
            ["Name"] = new
            {
                title = new[] { new { text = new { content = item.Title } } }
            },
            ["DataTypes"] = new
            {
                multi_select = multiSelect(item.ItemDataTypes.Select(d => d.DataType?.Name ?? ""))
            },
            ["Topics"] = new
            {
                multi_select = multiSelect(item.ItemTopics.Select(t => t.Topic?.Name ?? ""))
            },
            ["Tags"] = new
            {
                multi_select = multiSelect(tags)
            }
        };

        if (!string.IsNullOrWhiteSpace(item.SourceUrl))
            properties["Source"] = new { url = item.SourceUrl };

        if (!string.IsNullOrWhiteSpace(item.SourceRef))
            properties["SourceRef"] = new
            {
                rich_text = new[] { new { text = new { content = item.SourceRef } } }
            };

        if (!string.IsNullOrWhiteSpace(item.Author))
            properties["Author"] = new
            {
                rich_text = new[] { new { text = new { content = item.Author } } }
            };

        if (item.PublishedAt.HasValue)
            properties["Published"] = new
            {
                date = new { start = item.PublishedAt.Value.ToString("yyyy-MM-dd") }
            };

        var children = new List<object>();
        if (!string.IsNullOrWhiteSpace(item.Summary))
            children.Add(new
            {
                type = "paragraph",
                paragraph = new
                {
                    rich_text = new[] { new { text = new { content = item.Summary[..Math.Min(2000, item.Summary.Length)] } } }
                }
            });

        return new { parent = new { database_id = databaseId }, properties, children };
    }

    private static string TruncateError(string s) =>
        s.Length > 120 ? s[..120] + "…" : s;
}
