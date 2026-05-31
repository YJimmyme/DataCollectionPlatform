# 資訊收集平台 開發規格書

**版本**：v1.1  
**日期**：2026-05-31  
**形式**：本機個人工具（瀏覽器操作，資料存於本機）

---

## 1. 專案概述

### 1.1 目標

建構一個在本機執行的資訊收集小工具。執行一個指令後用瀏覽器開啟，即可管理、分類、搜尋自己收集的各類資料，並附上來源網址或出處以供日後查證。

### 1.2 核心特點

- **零部署負擔**：只需 `python app.py`，瀏覽器自動開啟
- **資料可攜**：所有資料存在一個 `data.db` 檔，複製即可備份
- **無需帳號**：個人使用，無登入機制
- **雙軸分類**：資料類別 × 資訊種類，兩個維度自由組合
- **來源強制**：每筆資料必須附上網址或出處

---

## 2. 功能需求

### 2.1 資料類別（Data Type）

可自由新增 / 修改，系統預設：

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

支援樹狀層級（主題 → 子主題），可自由新增，系統預設：

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

能源
  ├─ 太陽能
  ├─ 氫能
  └─ 儲能

生醫
  ├─ 醫療器材
  ├─ 藥物開發
  └─ 基因工程
```

### 2.3 資料項目欄位

| 欄位 | 必填 | 說明 |
|---|---|---|
| 標題 | ✅ | 資料標題 |
| 摘要 | ❌ | 重點節錄 |
| 資料類別 | ✅ | 至少一個（可多選）|
| 資訊種類 | ✅ | 至少一個（可多選）|
| 來源網址 | 條件必填 | 與「出處說明」擇一必填 |
| 出處說明 | 條件必填 | 期刊名 / 書名 / 報社等 |
| 作者 / 單位 | ❌ | |
| 發布日期 | ❌ | |
| 標籤 | ❌ | 自由文字標籤，逗號分隔 |
| 備註 | ❌ | 個人筆記 |
| 建立時間 | 自動 | 系統填入 |

> **驗證規則**：來源網址與出處說明至少填一項，否則無法儲存。

### 2.4 搜尋與篩選

- 關鍵字搜尋（比對標題、摘要、標籤、備註）
- 依資料類別篩選（多選）
- 依資訊種類篩選（多選，含子主題）
- 依發布日期區間篩選
- 排序：建立時間（新→舊）、發布日期、標題

### 2.5 其他功能

- **URL 快速填入**：貼上網址後自動嘗試擷取標題（可手動覆蓋）
- **批次匯入**：上傳 CSV 檔新增多筆資料
- **匯出**：將篩選結果匯出為 CSV 或 JSON
- **類別 / 主題管理**：在設定頁新增 / 修改 / 刪除類別與主題

---

## 3. 技術架構

### 3.1 技術選型

| 層級 | 技術 | 說明 |
|---|---|---|
| 後端 | **Python 3.11 + FastAPI** | 輕量、自帶 API 文件 |
| 資料庫 | **SQLite（單一 .db 檔）** | 免安裝、易備份 |
| ORM | **SQLModel** | FastAPI 原生搭配，同時做資料驗證 |
| 前端 | **HTML + Alpine.js + Tailwind CSS（CDN）** | 免打包、直接用瀏覽器跑 |
| URL 擷取 | **httpx + BeautifulSoup4** | 抓取網頁 meta 資訊 |
| 啟動 | **uvicorn** | `python app.py` 一行啟動 |

### 3.2 專案目錄結構

```
DataCollectionPlatform/
├── app.py               # 入口：啟動 FastAPI + 自動開啟瀏覽器
├── database.py          # SQLite 連線、資料表初始化
├── models.py            # SQLModel 資料模型
├── routers/
│   ├── items.py         # 資料 CRUD API
│   ├── types.py         # 資料類別管理 API
│   ├── topics.py        # 資訊種類管理 API
│   └── utils.py         # URL 擷取 API
├── static/
│   └── app.js           # 前端互動邏輯（Alpine.js）
├── templates/
│   ├── index.html       # 列表 / 搜尋頁
│   ├── form.html        # 新增 / 編輯頁
│   ├── detail.html      # 詳情頁
│   └── settings.html    # 類別 / 主題管理
├── data.db              # SQLite 資料庫（自動產生）
└── requirements.txt
```

### 3.3 資料庫 Schema（SQLite）

```sql
-- 資料類別
CREATE TABLE data_types (
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    name       TEXT NOT NULL UNIQUE,
    is_active  INTEGER DEFAULT 1
);

-- 資訊種類（樹狀）
CREATE TABLE topics (
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    name       TEXT NOT NULL,
    parent_id  INTEGER REFERENCES topics(id),
    is_active  INTEGER DEFAULT 1
);

-- 資料主表
CREATE TABLE items (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    title        TEXT NOT NULL,
    summary      TEXT,
    source_url   TEXT,
    source_ref   TEXT,
    author       TEXT,
    published_at TEXT,
    tags         TEXT,   -- 逗號分隔字串
    notes        TEXT,
    created_at   TEXT DEFAULT (datetime('now','localtime')),
    updated_at   TEXT DEFAULT (datetime('now','localtime')),
    -- 來源至少一項必填（應用層驗證）
    CHECK (source_url IS NOT NULL OR source_ref IS NOT NULL)
);

-- 資料 ↔ 類別（多對多）
CREATE TABLE item_data_types (
    item_id      INTEGER REFERENCES items(id) ON DELETE CASCADE,
    data_type_id INTEGER REFERENCES data_types(id),
    PRIMARY KEY (item_id, data_type_id)
);

-- 資料 ↔ 主題（多對多）
CREATE TABLE item_topics (
    item_id  INTEGER REFERENCES items(id) ON DELETE CASCADE,
    topic_id INTEGER REFERENCES topics(id),
    PRIMARY KEY (item_id, topic_id)
);
```

### 3.4 API 端點

```
# 資料
GET    /api/items              列表（?q=&type=&topic=&page=）
POST   /api/items              新增
GET    /api/items/{id}         取得單筆
PUT    /api/items/{id}         更新
DELETE /api/items/{id}         刪除
GET    /api/items/export       匯出 CSV / JSON
POST   /api/items/import       批次匯入 CSV

# 工具
POST   /api/fetch-url          { url } → 返回自動擷取的標題

# 資料類別
GET    /api/data-types
POST   /api/data-types
PUT    /api/data-types/{id}
DELETE /api/data-types/{id}

# 資訊種類
GET    /api/topics             返回樹狀結構
POST   /api/topics
PUT    /api/topics/{id}
DELETE /api/topics/{id}
```

---

## 4. 使用者介面

### 4.1 列表頁（首頁）

```
┌─────────────────────────────────────────────────────┐
│  資訊收集平台                      [+ 新增] [設定]  │
├──────────────┬──────────────────────────────────────┤
│              │  🔍 搜尋...                [匯出▼]   │
│ 資料類別     ├──────────────────────────────────────┤
│ □ 全部       │                                      │
│ ■ 文章 (12)  │  機器人視覺系統最新進展              │
│ □ 新聞  (8)  │  [論文] [機器人 > 工業機器人]        │
│ □ 論文  (5)  │  https://example.com   2026-03-15    │
│              │  ─────────────────────────────────── │
│ 資訊種類     │  功能性紡織品市場報告                │
│ ▶ 機器人     │  [報告] [紡織 > 功能性纖維]          │
│ ▼ 紡織       │  出處：紡織產業綜合研究所  2025-12   │
│   ■ 智慧紡織 │                                      │
│   □ 染整     │         [ 1  2  3 … ]                │
│ ▶ 材料       │                                      │
└──────────────┴──────────────────────────────────────┘
```

### 4.2 新增 / 編輯頁

- 頂部「貼上網址」欄位，按 Enter 自動填入標題
- 資料類別：多選 Checkbox
- 資訊種類：樹狀多選（展開 / 收合）
- 來源網址 / 出處說明至少一項，若都空白顯示紅色提示

### 4.3 詳情頁

- 顯示所有欄位
- 來源網址旁有「開啟連結」按鈕
- 右上角「編輯」「刪除」按鈕

---

## 5. 啟動方式（最終目標）

```bash
# 第一次使用
pip install -r requirements.txt

# 每次啟動（自動開啟瀏覽器）
python app.py
# → 瀏覽器自動開啟 http://localhost:8000
```

---

## 6. 開發階段規劃

### Phase 1（核心功能，約 2 週）
- [ ] 資料庫初始化與預設資料（類別、主題）
- [ ] 資料 CRUD API
- [ ] 基本列表、新增、編輯、詳情頁面
- [ ] 分類篩選與關鍵字搜尋
- [ ] 來源欄位必填驗證

### Phase 2（體驗優化，約 1 週）
- [ ] URL 自動擷取標題
- [ ] 類別 / 主題管理設定頁
- [ ] CSV 匯入 / 匯出
- [ ] 啟動時自動開啟瀏覽器

### Phase 3（加分功能，視需求）
- [ ] 全文搜尋加強（FTS5）
- [ ] 資料統計儀表板
- [ ] 附件上傳（PDF、圖片）
- [ ] 匯出 Markdown / JSON

---

## 7. 驗收標準

1. `python app.py` 即可啟動，無需額外設定
2. 每筆資料必須包含至少一個類別、一個主題、一個來源（網址或出處），否則拒絕儲存
3. 類別與主題可在設定頁自由新增，無需改程式碼
4. 可用類別 + 主題 + 關鍵字進行複合篩選
5. 所有資料可匯出為 CSV 備份

---

*本規格書確定後即可開始實作 Phase 1。*
