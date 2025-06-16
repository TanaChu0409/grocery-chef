# GroceryChef

GroceryChef 是一個以 ASP.NET Core 8 為基礎，支援 RESTful API 與 API 版本管理的食譜與採買清單管理系統。專案採用 Entity Framework Core 搭配 PostgreSQL，並支援 Docker 部署。

## 目錄結構

```
GroceryChef/
  GroceryChef.Api/         # ASP.NET Core Web API 專案
    Controllers/           # API 控制器
    Entities/              # 資料實體
    Database/              # EF Core 設定與遷移
    DTOs/                  # 資料傳輸物件
    Migrations/            # EF Core 遷移檔
    ...
  docker-compose.yml       # Docker Compose 設定
  GroceryChef.sln          # Visual Studio 解決方案
.github/
  workflows/
    main.yml               # CI/CD 工作流程
```

## 主要功能

- 使用者註冊、登入與權限管理（基於 ASP.NET Identity）
- 食材、食譜、購物車 CRUD 與資料分頁、排序、搜尋
- 支援 API 版本管理與 HATEOAS
- EF Core + PostgreSQL 資料庫
- Docker 化部署
- CI/CD (GitHub Actions)

## 快速開始

### 1. 環境需求

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- [PostgreSQL](https://www.postgresql.org/)（可用 Docker 啟動）

### 2. 開發環境啟動

```sh
git clone <本專案網址>
cd grocery-chef
dotnet restore GroceryChef/GroceryChef.sln
dotnet build GroceryChef/GroceryChef.sln
dotnet run --project GroceryChef/GroceryChef.Api
```

### 3. 使用 Docker Compose

```sh
docker compose up --build
```

### 4. 執行資料庫遷移

開發模式下會自動執行遷移與種子資料（見 [`Program.cs`](GroceryChef/GroceryChef.Api/Program.cs)）。

手動執行：
```sh
dotnet ef database update --project GroceryChef/GroceryChef.Api
```

## API 文件

啟動後可透過 Swagger UI 查看 API 文件：

- http://localhost:8080/swagger

## 測試

```sh
dotnet test GroceryChef/GroceryChef.sln
```

## CI/CD

CI/CD 設定於 [`.github/workflows/main.yml`](.github/workflows/main.yml)，包含自動建置、測試與發佈。

## 相關檔案與資料夾

- [`GroceryChef.Api/Controllers`](GroceryChef/GroceryChef.Api/Controllers)：API 控制器
- [`GroceryChef.Api/Entities`](GroceryChef/GroceryChef.Api/Entities)：資料實體
- [`GroceryChef.Api/Database`](GroceryChef/GroceryChef.Api/Database)：DbContext 與資料庫設定
- [`GroceryChef.Api/Migrations`](GroceryChef/GroceryChef.Api/Migrations)：EF Core 遷移
- [`GroceryChef.Api/DTOs`](GroceryChef/GroceryChef.Api/DTOs)：資料傳輸物件

## 授權

MIT License

---

如需協助或有任何問題，請開 issue 或聯絡專案維護者。