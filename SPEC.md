# 資訊收集平台 開發規格書

**版本**：v1.2  
**日期**：2026-05-31  
**形式**：本機個人工具（ASP.NET Core MVC，瀏覽器操作，資料存於本機）

---

## 1. 專案概述

### 1.1 目標

建構一個在本機執行的資訊收集小工具，使用熟悉的 **C# ASP.NET Core MVC** 架構開發。執行後用瀏覽器開啟，即可管理、分類、搜尋自己收集的各類資料，並附上來源網址或出處以供日後查證。

### 1.2 核心特點

- **熟悉架構**：標準 MVC 分層，Controller / Model / View 對應清楚
- **零部署負擔**：`dotnet run` 或直接執行 `.exe` 即可啟動
- **資料可攜**：所有資料存在本機 SQLite `.db` 檔，複製即備份
- **無需帳號**：個人使用，無登入機制
- **雙軸分類**：資料類別 × 資訊種類，兩個維度自由組合
- **來源強制**：每筆資料必須附上網址或出處

---

## 2. 功能需求

### 2.1 資料類別（DataType）

可在設定頁自由新增 / 修改，系統預設：

| 類別 | 說明 |
|---|---|
| 文章 | 部落格、專欄文章 |
| 新聞 | 新聞報導 |
| 論文 | 學術期刊、會議論文 |
| 報告 | 產業報告、白皮書 |
| 專利 | 專利文件 |
| 標準規範 | 國際 / 國家標準 |
| 書籍 | 教科書、專書 |
| 影音 | 影片、Podcast |
| 其他 | 不符合以上分類 |

### 2.2 資訊種類（Topic）

支援樹狀層級（主題 → 子主題），可在設定頁自由新增，系統預設：

```
機器人
  ├─ 工業機器人
  ├─ 協作機器人
  └─ 無人機
紡織
  ├─ 智慧紡織
  ├─ 功能性纖維
  └─ 染整製程
材料
  ├─ 複合材料
  ├─ 奈米材料
  └─ 生醫材料
化工
  ├─ 高分子
  ├─ 觸媒
  └─ 製程工程
AI / 機器學習
  ├─ 自然語言處理
  ├─ 電腦視覺
  └─ 強化學習
能源 / 生醫（依此類推）
```

### 2.3 資料項目欄位

| 欄位 | C# 屬性名稱 | 必填 | 說明 |
|---|---|---|---|
| 標題 | `Title` | ✅ | 最長 500 字 |
| 摘要 | `Summary` | ❌ | 重點節錄 |
| 來源網址 | `SourceUrl` | 條件必填 | 與 `SourceRef` 擇一必填 |
| 出處說明 | `SourceRef` | 條件必填 | 期刊名 / 書名 / 報社 |
| 頁碼 / 章節 | `SourcePage` | ❌ | |
| 作者 / 單位 | `Author` | ❌ | |
| 發布日期 | `PublishedAt` | ❌ | `DateTime?` |
| 標籤 | `Tags` | ❌ | 逗號分隔字串 |
| 備註 | `Notes` | ❌ | 個人筆記 |
| 資料類別 | `DataTypes` | ✅ | 多對多，至少一個 |
| 資訊種類 | `Topics` | ✅ | 多對多，至少一個 |
| 建立時間 | `CreatedAt` | 自動 | `DateTime` |
| 更新時間 | `UpdatedAt` | 自動 | `DateTime` |

> **驗證規則**：`SourceUrl` 與 `SourceRef` 兩者皆空時，Model Validation 回傳錯誤，拒絕儲存。

### 2.4 搜尋與篩選

- 關鍵字搜尋（比對 Title、Summary、Tags、Notes）
- 依資料類別篩選（多選 checkbox）
- 依資訊種類篩選（多選，含子主題向下展開）
- 依發布日期區間篩選
- 排序：建立時間（新→舊）、發布日期、標題字母順序

### 2.5 其他功能

| 功能 | 說明 |
|---|---|
| URL 快速填入 | 貼入網址後 AJAX 呼叫後端自動擷取標題（可手動覆蓋）|
| 批次匯入 | 上傳 CSV 檔，欄位對應後批次新增 |
| 匯出 | 將篩選結果匯出為 CSV 或 JSON |
| 設定頁 | 管理資料類別與資訊種類（新增 / 修改 / 停用）|

---

## 3. 技術架構

### 3.1 技術選型

| 層級 | 技術 | 說明 |
|---|---|---|
| 框架 | **ASP.NET Core 8 MVC** | 標準 MVC 架構，Razor Views |
| 語言 | **C# 12** | |
| ORM | **Entity Framework Core 8** | Code First，Migrations |
| 資料庫 | **SQLite**（`Microsoft.EntityFrameworkCore.Sqlite`）| 本機單一 `.db` 檔，免安裝 DB Server |
| 前端 | **Bootstrap 5 + jQuery** | 隨 ASP.NET Core 預設範本，熟悉 |
| URL 擷取 | **HttpClient + HtmlAgilityPack** | 抓取網頁 `<title>` / meta 資訊 |
| 驗證 | **Data Annotations + Tag Helpers** | 伺服器端 + 前端雙重驗證 |
| 啟動 | **`dotnet run`** | 或直接 publish 成 `.exe` |

### 3.2 專案目錄結構

```
DataCollectionPlatform/
├── Controllers/
│   ├── HomeController.cs          # 首頁 / 儀表板
│   ├── ItemsController.cs         # 資料 CRUD + 搜尋
│   ├── SettingsController.cs      # 類別 / 主題管理
│   └── ApiController.cs           # AJAX 端點（URL 擷取、匯入匯出）
│
├── Models/
│   ├── Item.cs                    # 資料主表 Entity
│   ├── DataType.cs                # 資料類別 Entity
│   ├── Topic.cs                   # 資訊種類 Entity（樹狀）
│   ├── ItemDataType.cs            # 多對多關聯表
│   ├── ItemTopic.cs               # 多對多關聯表
│   └── ViewModels/
│       ├── ItemIndexViewModel.cs  # 列表頁（含篩選條件）
│       ├── ItemFormViewModel.cs   # 新增 / 編輯表單
│       └── ItemDetailViewModel.cs # 詳情頁
│
├── Data/
│   └── AppDbContext.cs            # DbContext + Seed 預設資料
│
├── Services/
│   ├── IUrlFetchService.cs
│   ├── UrlFetchService.cs         # HttpClient 擷取網頁標題
│   ├── IImportExportService.cs
│   └── ImportExportService.cs     # CSV 匯入 / 匯出邏輯
│
├── Views/
│   ├── Items/
│   │   ├── Index.cshtml           # 列表 + 篩選
│   │   ├── Create.cshtml          # 新增表單
│   │   ├── Edit.cshtml            # 編輯表單
│   │   └── Detail.cshtml          # 詳情
│   ├── Settings/
│   │   ├── Index.cshtml           # 類別 / 主題管理
│   │   └── _TopicTree.cshtml      # 樹狀主題元件（partial）
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _FilterPanel.cshtml    # 左側篩選面板（partial）
│
├── wwwroot/
│   ├── css/site.css
│   └── js/site.js                 # URL 自動擷取、樹狀展開邏輯
│
├── Migrations/                    # EF Core 自動產生
├── appsettings.json               # 連線字串（SQLite 路徑）
├── Program.cs
└── DataCollectionPlatform.csproj
```

### 3.3 資料模型（C# Entity）

```csharp
// Models/Item.cs
public class Item
{
    public int Id { get; set; }

    [Required, StringLength(500)]
    public string Title { get; set; } = "";

    public string? Summary { get; set; }
    public string? SourceUrl { get; set; }
    public string? SourceRef { get; set; }
    public string? SourcePage { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Tags { get; set; }       // 逗號分隔
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public ICollection<ItemDataType> ItemDataTypes { get; set; } = [];
    public ICollection<ItemTopic> ItemTopics { get; set; } = [];
}

// Models/DataType.cs
public class DataType
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    public bool IsActive { get; set; } = true;
    public ICollection<ItemDataType> ItemDataTypes { get; set; } = [];
}

// Models/Topic.cs
public class Topic
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    public int? ParentId { get; set; }
    public Topic? Parent { get; set; }
    public ICollection<Topic> Children { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public ICollection<ItemTopic> ItemTopics { get; set; } = [];
}

// Models/ItemDataType.cs（多對多）
public class ItemDataType
{
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public int DataTypeId { get; set; }
    public DataType DataType { get; set; } = null!;
}

// Models/ItemTopic.cs（多對多）
public class ItemTopic
{
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
}
```

### 3.4 自訂驗證（來源必填）

```csharp
// 自訂 Validation Attribute，套用在 ItemFormViewModel
public class SourceRequiredAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var vm = (ItemFormViewModel)ctx.ObjectInstance;
        if (string.IsNullOrWhiteSpace(vm.SourceUrl) &&
            string.IsNullOrWhiteSpace(vm.SourceRef))
        {
            return new ValidationResult("來源網址或出處說明至少填寫一項");
        }
        return ValidationResult.Success;
    }
}
```

### 3.5 DbContext

```csharp
// Data/AppDbContext.cs
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<DataType> DataTypes => Set<DataType>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<ItemDataType> ItemDataTypes => Set<ItemDataType>();
    public DbSet<ItemTopic> ItemTopics => Set<ItemTopic>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ItemDataType>().HasKey(x => new { x.ItemId, x.DataTypeId });
        mb.Entity<ItemTopic>().HasKey(x => new { x.ItemId, x.TopicId });

        // Seed 預設資料類別
        mb.Entity<DataType>().HasData(
            new DataType { Id = 1, Name = "文章" },
            new DataType { Id = 2, Name = "新聞" },
            new DataType { Id = 3, Name = "論文" },
            new DataType { Id = 4, Name = "報告" },
            new DataType { Id = 5, Name = "專利" },
            new DataType { Id = 6, Name = "標準規範" },
            new DataType { Id = 7, Name = "書籍" },
            new DataType { Id = 8, Name = "影音" },
            new DataType { Id = 9, Name = "其他" }
        );

        // Seed 預設資訊種類（部分示範）
        mb.Entity<Topic>().HasData(
            new Topic { Id = 1, Name = "機器人",     ParentId = null },
            new Topic { Id = 2, Name = "工業機器人", ParentId = 1 },
            new Topic { Id = 3, Name = "協作機器人", ParentId = 1 },
            new Topic { Id = 4, Name = "紡織",       ParentId = null },
            new Topic { Id = 5, Name = "智慧紡織",   ParentId = 4 }
            // ... 其餘依此類推
        );
    }
}
```

### 3.6 Program.cs 設定

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHttpClient<IUrlFetchService, UrlFetchService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();

var app = builder.Build();

// 啟動時自動套用 Migration
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Items}/{action=Index}/{id?}");

// 開發時自動開啟瀏覽器
if (app.Environment.IsDevelopment())
{
    Task.Run(() =>
    {
        System.Threading.Thread.Sleep(1500);
        var url = "http://localhost:5000";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    });
}

app.Run();
```

### 3.7 appsettings.json

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=data.db"
  },
  "Urls": "http://localhost:5000"
}
```

---

## 4. 使用者介面

### 4.1 列表頁（Items/Index）

```
┌─────────────────────────────────────────────────────┐
│  資訊收集平台                      [+ 新增] [設定]  │
├──────────────┬──────────────────────────────────────┤
│              │  🔍 [搜尋關鍵字...]      [匯出 CSV]  │
│ 資料類別     ├──────────────────────────────────────┤
│ □ 全部       │  機器人視覺系統最新進展              │
│ ■ 文章 (12)  │  [論文][機器人 > 工業機器人]         │
│ □ 新聞  (8)  │  🔗 https://...   作者：王小明        │
│ □ 論文  (5)  │  2026-03-15  ─────────────────────── │
│              │  功能性紡織品市場報告 2025            │
│ 資訊種類     │  [報告][紡織 > 功能性纖維]           │
│ ▶ 機器人     │  出處：紡織產業綜合研究所            │
│ ▼ 紡織       │                                      │
│   ■ 智慧紡織 │         [ ← 1  2  3 → ]              │
│   □ 染整     │                                      │
│ ▶ 材料       │                                      │
└──────────────┴──────────────────────────────────────┘
```

### 4.2 新增 / 編輯頁（Items/Create, Edit）

- 頂部「貼上網址」欄位：失焦時 AJAX 呼叫 `/api/fetch-url`，自動填入標題
- 資料類別：`CheckBoxList`（Partial View）
- 資訊種類：樹狀展開多選（`<ul>` 巢狀結構 + jQuery toggle）
- 來源網址 / 出處：前端 JavaScript 額外驗證，提交前確認至少填一項
- 使用 Bootstrap 5 表單 layout，Tag Helpers 處理驗證訊息

### 4.3 詳情頁（Items/Detail）

- 完整顯示所有欄位與標籤
- 來源網址旁「開啟連結」按鈕（`target="_blank"`）
- 右上角「編輯」「刪除」按鈕（刪除以 Modal 確認）

### 4.4 設定頁（Settings/Index）

- 左側 Tab：資料類別管理 / 資訊種類管理
- 資料類別：簡易列表 + 新增 / 修改 / 停用（Bootstrap inline form）
- 資訊種類：樹狀結構 + 新增子主題 / 修改名稱 / 停用

---

## 5. 啟動與部署

### 開發執行

```bash
cd DataCollectionPlatform
dotnet run
# 瀏覽器自動開啟 http://localhost:5000
```

### 發行成單一執行檔（選用）

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
# 產出 DataCollectionPlatform.exe，雙擊即可執行
```

### 資料備份

```bash
# 只需複製 data.db 即可備份所有資料
copy data.db backup\data_20260531.db
```

---

## 6. 開發階段規劃

### Phase 1（核心功能，約 2 週）

- [ ] 建立 ASP.NET Core MVC 專案、安裝套件
- [ ] Entity + DbContext + Migration + Seed 預設資料
- [ ] Items CRUD（Index / Create / Edit / Detail / Delete）
- [ ] 來源欄位自訂驗證
- [ ] 左側分類篩選面板（DataType + Topic）
- [ ] 關鍵字搜尋

### Phase 2（體驗優化，約 1 週）

- [ ] URL 自動擷取標題（HttpClient + HtmlAgilityPack）
- [ ] Settings 頁：資料類別 / 資訊種類 CRUD
- [ ] CSV 匯入 / 匯出（CsvHelper 套件）
- [ ] 啟動時自動開啟瀏覽器

### Phase 3（加分，視需求）

- [ ] 全文搜尋強化（EF.Functions.Like 多欄位）
- [ ] 統計儀表板（各類別數量長條圖）
- [ ] 附件上傳（PDF / 圖片，存於本機資料夾）

---

## 7. 主要 NuGet 套件

| 套件 | 用途 |
|---|---|
| `Microsoft.EntityFrameworkCore.Sqlite` | SQLite 資料庫 |
| `Microsoft.EntityFrameworkCore.Tools` | Migrations CLI |
| `HtmlAgilityPack` | 擷取網頁標題 |
| `CsvHelper` | CSV 匯入 / 匯出 |

---

## 8. 驗收標準

1. `dotnet run` 即可啟動，無需額外安裝資料庫
2. 每筆資料必須包含至少一個類別、一個主題、一個來源（網址或出處），否則 Model Validation 拒絕儲存
3. 類別與主題可在設定頁自由新增，無需改程式碼或重新編譯
4. 可用類別 + 主題 + 關鍵字進行複合篩選
5. 所有資料可匯出為 CSV 備份

---

*規格書確認後即可開始實作 Phase 1。*
