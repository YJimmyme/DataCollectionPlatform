using DataCollectionPlatform.Models;

namespace DataCollectionPlatform.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.DataTypes.Any()) return;

        var dataTypes = new[]
        {
            "文章", "新聞", "論文", "報告", "專利", "標準規範", "書籍", "影音", "其他"
        }.Select(n => new DataType { Name = n }).ToList();

        db.DataTypes.AddRange(dataTypes);
        db.SaveChanges();

        var topics = new List<(string Name, string? Parent)>
        {
            ("機器人", null),
            ("工業機器人", "機器人"),
            ("協作機器人", "機器人"),
            ("無人機", "機器人"),

            ("紡織", null),
            ("智慧紡織", "紡織"),
            ("功能性纖維", "紡織"),
            ("染整製程", "紡織"),

            ("材料", null),
            ("複合材料", "材料"),
            ("奈米材料", "材料"),
            ("生醫材料", "材料"),

            ("化工", null),
            ("高分子", "化工"),
            ("觸媒", "化工"),
            ("製程工程", "化工"),

            ("AI / 機器學習", null),
            ("自然語言處理", "AI / 機器學習"),
            ("電腦視覺", "AI / 機器學習"),
            ("強化學習", "AI / 機器學習"),

            ("能源", null),
            ("太陽能", "能源"),
            ("氫能", "能源"),
            ("儲能", "能源"),

            ("生醫", null),
            ("醫療器材", "生醫"),
            ("藥物開發", "生醫"),
            ("基因工程", "生醫"),
        };

        var savedRoots = new Dictionary<string, Topic>();

        foreach (var (name, parent) in topics)
        {
            var t = new Topic { Name = name };
            if (parent != null && savedRoots.TryGetValue(parent, out var p))
                t.ParentId = p.Id;

            db.Topics.Add(t);
            db.SaveChanges();

            if (parent == null)
                savedRoots[name] = t;
        }

        // 推送平台預設設定
        if (!db.AppSettings.Any())
        {
            db.AppSettings.AddRange(
                new AppSetting { Key = "notion.api_token",    Value = "" },
                new AppSetting { Key = "notion.database_id",  Value = "" },
                new AppSetting { Key = "email.smtp_host",     Value = "smtp.gmail.com" },
                new AppSetting { Key = "email.smtp_port",     Value = "587" },
                new AppSetting { Key = "email.username",      Value = "" },
                new AppSetting { Key = "email.password",      Value = "" },
                new AppSetting { Key = "email.from_address",  Value = "" },
                new AppSetting { Key = "email.to_addresses",  Value = "" }
            );
            db.SaveChanges();
        }
    }
}
