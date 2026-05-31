# 資訊收集平台

個人本機資訊管理工具，ASP.NET Core 8 MVC + SQLite。

## 安裝與啟動

```bash
# 1. 安裝相依套件
dotnet restore

# 2. 啟動（瀏覽器自動開啟 http://localhost:5000）
dotnet run
```

> 第一次啟動會自動建立 `data.db` 並填入預設的資料類別與主題。

## 資料備份

```bash
# 複製 data.db 即可備份全部資料
copy data.db backup\data_20260531.db
```

## 發行成單一執行檔（選用）

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## 功能說明

| 功能 | 說明 |
|---|---|
| 新增資料 | 貼上網址自動擷取標題，雙軸分類（類別 + 主題） |
| 來源驗證 | URL 或出處說明至少填一項，拒絕空白儲存 |
| 搜尋篩選 | 關鍵字 + 類別 + 主題 + 日期複合篩選 |
| 匯入匯出 | CSV 格式批次匯入、篩選結果匯出 |
| 設定管理 | 在「設定」頁自由新增 / 修改類別與主題 |

## 技術

- ASP.NET Core 8 MVC
- Entity Framework Core 8 + SQLite
- Bootstrap 5 + jQuery
- HtmlAgilityPack（網頁標題擷取）
- CsvHelper（CSV 匯入匯出）
