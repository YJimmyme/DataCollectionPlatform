# 資訊收集平台 開發規格書

**版本**：v1.0  
**日期**：2026-05-31  
**狀態**：草稿

---

## 1. 專案概述

### 1.1 目標

建構一個可擴展的資訊收集平台，讓使用者能夠以「資料類別」（如文章、新聞、論文）與「資訊種類」（如機器人、紡織、材料）兩個維度進行分類管理，並完整保存每筆資料的來源出處以供查證。

### 1.2 核心功能摘要

| 功能面向 | 說明 |
|---|---|
| 資料收集 | 手動新增 / 自動爬取 |
| 分類管理 | 資料類別 × 資訊種類 雙軸分類，支援自訂擴充 |
| 來源管理 | 強制附上 URL 或出處資訊 |
| 搜尋查詢 | 全文搜尋、多條件篩選 |
| 使用者管理 | 多使用者、角色權限 |
| 匯出報表 | CSV / JSON / PDF |

---

## 2. 功能需求

### 2.1 資料類別（Data Type）

系統預設以下類別，並支援管理者新增自訂類別：

| 類別 ID | 類別名稱 |
|---|---|
| article | 文章 |
| news | 新聞 |
| paper | 論文 |
| report | 報告 |
| patent | 專利 |
| standard | 標準規範 |
| book | 書籍 |
| video | 影音 |
| other | 其他 |

**擴充機制**：管理者可在後台新增 / 修改 / 停用類別，類別與資料為多對多關係（一筆資料可標記多個類別）。

### 2.2 資訊種類（Topic）

系統預設以下主題，並支援樹狀階層（主題 → 子主題）：

| 主題 | 範例子主題 |
|---|---|
| 機器人 | 工業機器人、協作機器人、無人機 |
| 紡織 | 功能性紡織品、智慧紡織、染整 |
| 材料 | 複合材料、奈米材料、生醫材料 |
| 化工 | 高分子、觸媒、製程工程 |
| AI / 機器學習 | 自然語言處理、電腦視覺、強化學習 |
| 能源 | 太陽能、氫能、儲能 |
| 生醫 | 醫療器材、藥物開發、基因工程 |

**擴充機制**：使用者可自訂主題樹，支援無限層級的巢狀分類。

### 2.3 資料項目（Item）欄位定義

| 欄位 | 類型 | 必填 | 說明 |
|---|---|---|---|
| id | UUID | 系統 | 唯一識別碼 |
| title | String（最長 500） | ✅ | 標題 |
| summary | Text | ❌ | 摘要 / 重點節錄 |
| content | Text | ❌ | 全文或詳細內容 |
| source_url | URL | 條件必填 | 網路來源（與 source_ref 擇一必填） |
| source_ref | String | 條件必填 | 實體出處（期刊名、書名、報社等） |
| source_page | String | ❌ | 頁碼 / 章節 |
| author | String | ❌ | 作者 / 單位 |
| published_at | Date | ❌ | 原始發布日期 |
| language | String | ❌ | 語言（zh-TW、en、ja…） |
| data_types | Array[Type] | ✅ | 資料類別（至少一個） |
| topics | Array[Topic] | ✅ | 資訊種類（至少一個） |
| tags | Array[String] | ❌ | 自由標籤 |
| attachments | Array[File] | ❌ | 附件（PDF、圖片等） |
| created_by | UserID | 系統 | 建立者 |
| created_at | Timestamp | 系統 | 建立時間 |
| updated_at | Timestamp | 系統 | 最後更新時間 |
| status | Enum | 系統 | draft / published / archived |

> **來源驗證規則**：`source_url` 或 `source_ref` 兩者至少填寫一項，否則系統拒絕儲存。

### 2.4 搜尋與篩選

- 全文搜尋：標題、摘要、內容、標籤
- 篩選條件：
  - 資料類別（多選）
  - 資訊種類（多選，含子主題）
  - 日期區間（發布日期 / 建立日期）
  - 語言
  - 建立者
  - 狀態
- 排序：相關性、建立時間（新→舊/舊→新）、發布日期
- 分頁：每頁預設 20 筆，最大 100 筆

### 2.5 資料收集方式

#### 2.5.1 手動新增
使用者透過 Web 介面填寫表單，貼上 URL 後系統可自動擷取網頁標題與摘要（Open Graph / meta description）。

#### 2.5.2 URL 快速匯入
- 貼入 URL → 系統自動抓取標題、作者、發布日期
- 使用者確認後補充分類資訊即可儲存

#### 2.5.3 批次匯入
- 支援上傳 CSV / JSON 格式
- CSV 欄位對應說明文件隨平台提供
- 匯入時進行欄位驗證，產生錯誤報告

#### 2.5.4 排程爬取（進階，Phase 2）
- 設定 RSS Feed、關鍵字監控規則
- 定時自動收集並標記待審核

### 2.6 來源管理

- 每筆資料保存完整的 `source_url`（含擷取快照時間戳）
- 支援「存證截圖」附件上傳
- 提供「驗證連結」按鈕，直接開啟原始來源
- 若 URL 已失效，系統標記為「連結失效」並保留原始 URL 及快照

---

## 3. 使用者角色與權限

| 角色 | 新增資料 | 編輯自己 | 編輯他人 | 刪除 | 管理類別 | 管理使用者 |
|---|---|---|---|---|---|---|
| 訪客 | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| 一般使用者 | ✅ | ✅ | ❌ | 草稿 only | ❌ | ❌ |
| 編輯者 | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| 管理者 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## 4. 系統架構

### 4.1 技術選型（建議）

| 層級 | 技術選項 A（輕量） | 技術選項 B（企業級） |
|---|---|---|
| 前端 | Next.js 14 (App Router) | Next.js 14 + Nx monorepo |
| 後端 API | Next.js API Routes | FastAPI (Python) |
| 資料庫 | PostgreSQL | PostgreSQL + Elasticsearch |
| 全文搜尋 | PostgreSQL FTS | Elasticsearch |
| 快取 | Redis | Redis |
| 檔案儲存 | 本地 / S3 | AWS S3 / MinIO |
| 認證 | NextAuth.js | Keycloak / Auth0 |
| 部署 | Docker Compose | Kubernetes |

> **建議起點**：選項 A（輕量），待資料量超過 10 萬筆或多租戶需求出現再升級至選項 B。

### 4.2 資料庫 Schema（PostgreSQL）

```sql
-- 資料類別
CREATE TABLE data_types (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,
    slug        VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_active   BOOLEAN DEFAULT TRUE,
    created_at  TIMESTAMPTZ DEFAULT NOW()
);

-- 資訊種類（樹狀）
CREATE TABLE topics (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(200) NOT NULL,
    slug        VARCHAR(200) NOT NULL UNIQUE,
    parent_id   UUID REFERENCES topics(id),
    description TEXT,
    is_active   BOOLEAN DEFAULT TRUE,
    created_at  TIMESTAMPTZ DEFAULT NOW()
);

-- 資料主表
CREATE TABLE items (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title          VARCHAR(500) NOT NULL,
    summary        TEXT,
    content        TEXT,
    source_url     TEXT,
    source_ref     TEXT,
    source_page    VARCHAR(200),
    author         VARCHAR(300),
    published_at   DATE,
    language       VARCHAR(10),
    tags           TEXT[],
    status         VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft','published','archived')),
    created_by     UUID NOT NULL,
    created_at     TIMESTAMPTZ DEFAULT NOW(),
    updated_at     TIMESTAMPTZ DEFAULT NOW(),
    -- 全文搜尋向量
    search_vector  TSVECTOR GENERATED ALWAYS AS (
        to_tsvector('simple', coalesce(title,'') || ' ' || coalesce(summary,'') || ' ' || coalesce(content,''))
    ) STORED,
    CONSTRAINT source_required CHECK (source_url IS NOT NULL OR source_ref IS NOT NULL)
);

-- 資料 ↔ 類別（多對多）
CREATE TABLE item_data_types (
    item_id      UUID REFERENCES items(id) ON DELETE CASCADE,
    data_type_id UUID REFERENCES data_types(id),
    PRIMARY KEY (item_id, data_type_id)
);

-- 資料 ↔ 主題（多對多）
CREATE TABLE item_topics (
    item_id  UUID REFERENCES items(id) ON DELETE CASCADE,
    topic_id UUID REFERENCES topics(id),
    PRIMARY KEY (item_id, topic_id)
);

-- 索引
CREATE INDEX idx_items_search ON items USING GIN(search_vector);
CREATE INDEX idx_items_status ON items(status);
CREATE INDEX idx_items_published_at ON items(published_at DESC);
CREATE INDEX idx_topics_parent ON topics(parent_id);
```

### 4.3 API 端點設計（RESTful）

```
# 資料項目
GET    /api/items              列表（含搜尋、篩選、分頁）
POST   /api/items              新增
GET    /api/items/:id          取得單筆
PUT    /api/items/:id          更新
DELETE /api/items/:id          刪除
POST   /api/items/import       批次匯入
GET    /api/items/export       匯出（query 同列表）

# URL 快速擷取
POST   /api/fetch-url          { url } → 返回自動擷取的 metadata

# 資料類別
GET    /api/data-types         列表
POST   /api/data-types         新增（管理者）
PUT    /api/data-types/:id     更新（管理者）

# 資訊種類
GET    /api/topics             列表（樹狀結構）
POST   /api/topics             新增
PUT    /api/topics/:id         更新
DELETE /api/topics/:id         刪除（需無子節點）

# 認證
POST   /api/auth/login
POST   /api/auth/logout
GET    /api/auth/me
```

---

## 5. 使用者介面（UI/UX）

### 5.1 主要頁面

| 頁面 | 說明 |
|---|---|
| `/` 儀表板 | 統計卡片（總資料數、本週新增）、最近新增列表 |
| `/items` 列表頁 | 左側分類樹 + 右側卡片/表格切換，頂部搜尋列 |
| `/items/new` 新增頁 | 表單（支援 URL 自動擷取）|
| `/items/:id` 詳情頁 | 完整資訊 + 來源連結 + 編輯按鈕 |
| `/items/:id/edit` 編輯頁 | 同新增頁，帶入現有資料 |
| `/admin/data-types` | 資料類別管理 |
| `/admin/topics` | 資訊種類樹狀管理 |
| `/admin/users` | 使用者管理 |

### 5.2 列表頁篩選 UI

```
┌─────────────────────────────────────────────────────────┐
│ 🔍 搜尋關鍵字...                          [新增資料]   │
├──────────────┬──────────────────────────────────────────┤
│ 資料類別     │  排序：建立時間▼     每頁：20  [匯出]   │
│ ☑ 文章       ├──────────────────────────────────────────┤
│ ☑ 新聞       │ ┌──────────────────────────────────────┐ │
│ ☐ 論文       │ │ [標題] 機器人視覺系統最新進展         │ │
│ ☐ 報告       │ │ 類別：論文  主題：機器人 > 工業機器人 │ │
│ ☐ 專利       │ │ 來源：https://...  2026-03-15         │ │
│              │ └──────────────────────────────────────┘ │
│ 資訊種類     │ ┌──────────────────────────────────────┐ │
│ ▶ 機器人     │ │ ...                                  │ │
│ ▼ 紡織       │ └──────────────────────────────────────┘ │
│   ☑ 智慧紡織 │                                          │
│   ☐ 染整     │              < 1 2 3 ... >               │
│ ▶ 材料       │                                          │
└──────────────┴──────────────────────────────────────────┘
```

---

## 6. 非功能需求

### 6.1 效能

| 指標 | 目標 |
|---|---|
| 列表頁回應時間（P95） | < 500ms |
| 單筆新增回應時間 | < 1s |
| URL 自動擷取 | < 5s（逾時顯示錯誤，不阻塞儲存）|
| 搜尋回應（10 萬筆以內） | < 1s |

### 6.2 安全性

- 所有 API 端點需攜帶 JWT 驗證 Token（公開瀏覽視需求決定）
- 輸入欄位防 XSS（伺服器端 HTML sanitize）
- SQL 使用 parameterized query / ORM，防注入
- 上傳檔案：限制副檔名白名單、掃描大小限制（單檔 50MB）
- HTTPS 強制

### 6.3 可擴充性

- 資料類別與資訊種類完全動態，無 hardcode
- API 版本化（`/api/v1/...`）
- 支援多語系（i18n，初期中文 / 英文）

---

## 7. 開發階段規劃

### Phase 1（MVP，約 6 週）

- [ ] 資料庫設計與建立
- [ ] 後端 CRUD API（資料項目、類別、主題）
- [ ] 來源必填驗證
- [ ] 前端列表、新增、詳情頁
- [ ] 基本搜尋與分類篩選
- [ ] 使用者登入 / 登出（JWT）

### Phase 2（約 4 週）

- [ ] URL 自動擷取 metadata
- [ ] 批次 CSV / JSON 匯入
- [ ] 匯出功能（CSV / JSON）
- [ ] 角色權限完整實作
- [ ] 管理後台（類別 / 主題 / 使用者管理）

### Phase 3（約 4 週）

- [ ] 全文搜尋強化（Elasticsearch 或 pg_tsvector 調優）
- [ ] 排程爬取 / RSS 訂閱
- [ ] 失效連結偵測
- [ ] PDF 匯出報表
- [ ] 統計儀表板

---

## 8. 驗收標準

1. 每筆資料必須包含至少一個資料類別、一個資訊種類，以及 URL 或出處，否則系統拒絕儲存。
2. 資料類別與資訊種類均可由管理者在不修改程式碼的情況下新增 / 修改。
3. 資訊種類支援至少 3 層巢狀分類。
4. 搜尋功能可同時依類別、主題、關鍵字進行複合篩選。
5. 所有資料的來源 URL 可一鍵開啟驗證。

---

## 9. 待確認事項（需業主決策）

| 項目 | 選項 | 預設建議 |
|---|---|---|
| 是否開放公開瀏覽（未登入） | 是 / 否 | 否 |
| 是否支援多租戶（不同團隊獨立資料） | 是 / 否 | 否（單一組織）|
| URL 自動擷取是否保存快照 | 是 / 否 | 是 |
| 爬取功能是否列入 Phase 1 | 是 / 否 | 否 |
| 前端語言偏好 | 中文 / 英文 / 雙語 | 繁體中文 |

---

*本規格書為初稿，請業主確認後進入細部設計階段。*
