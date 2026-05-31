using System.Net;
using System.Net.Mail;
using System.Text;
using DataCollectionPlatform.Data;
using DataCollectionPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Services;

public class EmailService(AppDbContext db) : IEmailService
{
    public async Task<(bool Success, string Message)> SendItemsAsync(
        IEnumerable<Item> items, string scheduleName)
    {
        var cfg = await LoadConfigAsync();

        if (string.IsNullOrWhiteSpace(cfg.SmtpHost) ||
            string.IsNullOrWhiteSpace(cfg.Username)  ||
            string.IsNullOrWhiteSpace(cfg.ToAddresses))
            return (false, "Email SMTP 設定未完成，請至「設定 → 推送平台」填入");

        var itemList = items.ToList();
        var subject  = $"【資訊收集平台】{scheduleName} — {DateTime.Now:yyyy-MM-dd}（{itemList.Count} 筆）";
        var body     = BuildHtml(itemList, scheduleName);

        try
        {
            using var smtp = new SmtpClient(cfg.SmtpHost, cfg.SmtpPort)
            {
                EnableSsl   = true,
                Credentials = new NetworkCredential(cfg.Username, cfg.Password)
            };

            var mail = new MailMessage
            {
                From       = new MailAddress(cfg.FromAddress.Length > 0 ? cfg.FromAddress : cfg.Username),
                Subject    = subject,
                Body       = body,
                IsBodyHtml = true
            };

            foreach (var addr in cfg.ToAddresses
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                mail.To.Add(addr);

            await smtp.SendMailAsync(mail);
            return (true, $"已發送 {itemList.Count} 筆資料至 {cfg.ToAddresses}");
        }
        catch (Exception ex)
        {
            return (false, $"發送失敗：{ex.Message}");
        }
    }

    private static string BuildHtml(List<Item> items, string scheduleName)
    {
        var sb = new StringBuilder();
        sb.Append($"""
            <html><body style="font-family:sans-serif;color:#333">
            <h2 style="color:#0d6efd">資訊收集平台 · {scheduleName}</h2>
            <p>共 <strong>{items.Count}</strong> 筆資料 · 查詢時間：{DateTime.Now:yyyy-MM-dd HH:mm}</p>
            <table border="1" cellpadding="8" cellspacing="0"
                   style="border-collapse:collapse;width:100%;font-size:13px">
            <tr style="background:#e9ecef;font-weight:bold">
                <td>標題</td><td>類別</td><td>主題</td><td>來源</td><td>日期</td>
            </tr>
            """);

        foreach (var item in items)
        {
            var source = !string.IsNullOrWhiteSpace(item.SourceUrl)
                ? $"<a href='{item.SourceUrl}'>{Trunc(item.SourceUrl, 50)}</a>"
                : System.Net.WebUtility.HtmlEncode(item.SourceRef ?? "");

            var types  = string.Join(", ", item.ItemDataTypes.Select(d => d.DataType?.Name ?? ""));
            var topics = string.Join(", ", item.ItemTopics.Select(t => t.Topic?.Name ?? ""));
            var date   = item.PublishedAt?.ToString("yyyy-MM-dd") ?? item.CreatedAt.ToString("yyyy-MM-dd");

            sb.Append($"""
                <tr>
                    <td><strong>{System.Net.WebUtility.HtmlEncode(item.Title)}</strong>
                        {(string.IsNullOrWhiteSpace(item.Summary) ? "" :
                          $"<br><small style='color:#666'>{Trunc(item.Summary, 120)}</small>")}
                    </td>
                    <td>{types}</td>
                    <td>{topics}</td>
                    <td style="word-break:break-all">{source}</td>
                    <td style="white-space:nowrap">{date}</td>
                </tr>
                """);
        }

        sb.Append("</table></body></html>");
        return sb.ToString();
    }

    private async Task<EmailConfig> LoadConfigAsync()
    {
        var rows = await db.AppSettings
            .Where(s => s.Key.StartsWith("email."))
            .ToDictionaryAsync(s => s.Key, s => s.Value ?? "");

        return new EmailConfig(
            rows.GetValueOrDefault("email.smtp_host", "smtp.gmail.com"),
            int.TryParse(rows.GetValueOrDefault("email.smtp_port", "587"), out var p) ? p : 587,
            rows.GetValueOrDefault("email.username", ""),
            rows.GetValueOrDefault("email.password", ""),
            rows.GetValueOrDefault("email.from_address", ""),
            rows.GetValueOrDefault("email.to_addresses", "")
        );
    }

    private static string Trunc(string s, int max) =>
        s.Length > max ? s[..max] + "…" : s;

    private record EmailConfig(
        string SmtpHost, int SmtpPort,
        string Username, string Password,
        string FromAddress, string ToAddresses);
}
