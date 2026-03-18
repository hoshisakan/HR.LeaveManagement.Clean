## HR.LeaveManagement.Clean

[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)](ARCHITECTURE.md)
[![Pattern](https://img.shields.io/badge/Pattern-CQRS%20%2B%20MediatR-brightgreen)]()
[![Auth](https://img.shields.io/badge/Auth-JWT%20%2B%20Refresh%20Token-orange)]()
[![UI](https://img.shields.io/badge/Frontend-React%20%2F%20Vite%20%2F%20TS-61dafb)]()

**HR Leave Management System** – 一套示範請假管理的端到端解決方案。  
後端採用 **Clean Architecture + CQRS（MediatR）+ ASP.NET Core + EF Core**，前端採用 **React + TypeScript + Vite**，並以 **Docker Compose + Nginx + MSSQL** 模擬接近 Production 的環境。

---

### 🛡️ 核心業務邏輯與資料安全（Core Business Logic & Data Integrity）

為了模擬真實商用環境的嚴謹需求，本系統在架構層級實作了以下關鍵優化，強調 **配額正確性、審計軌跡（Audit Trail）與交易原子性**。

- **精確配額管理（Cumulative Balance Tracking）**  
  - 在 `LeaveAllocation` 實體中新增 `UsedDays` 欄位，實作動態追蹤員工每一種假別「已核准且未取消」的累計天數。  
  - 在核准（Approve）階段，`ChangeLeaveRequestApprovalCommandHandler` 會執行「累計餘額校驗」：  
    - 計算本次請假天數 `daysRequested`。  
    - 從 Repository 取得對應年度配額後，檢查 **(UsedDays + daysRequested) ≤ NumberOfDays**。  
    - 若將導致 Over-allocation（超過年度總配額），即拋出 `BadRequestException("Insufficient leave balance for this request.")`，從根本杜絕請假超支問題。

- **審計軌跡保護（Audit Trail Protection）**  
  - 在 EF Core 的關聯設定中，將 `LeaveRequest` / `LeaveAllocation` 對 `LeaveType` 的外鍵刪除行為，由預設 `DeleteBehavior.Cascade` 明確調整為 `DeleteBehavior.Restrict`。  
  - 此設計防止誤刪「已有關聯紀錄」的假別（LeaveType），避免歷史請假紀錄被級聯清空。  
  - 藉由這套 Referential Integrity 策略，系統能長期維持完整且可追溯（Traceable）的 Audit Trail，符合企業與法遵對歷史數據保留的要求。

- **交易原子性（Atomic Transactions via Unit of Work）**  
  - `HrDatabaseContext` 作為 Unit of Work，負責在單一資料庫交易中追蹤並提交 `LeaveRequest` 與 `LeaveAllocation` 的變更。  
  - 在核准請假流程中，「更新請假狀態（Approved）」與「更新配額餘額（UsedDays）」會在同一個 `SaveChangesAsync` 交易中完成：  
    - 任一更新失敗時整筆交易回滾，避免出現「狀態已核准但配額未扣除」或反向情況。  
  - 此設計大幅提升系統在高併發與多管理員審核場景下的資料一致性與可靠性。

---

### 系統架構概覽（Backend & Frontend）

- **後端（位於 `api/`）**  
  - `HR.LeaveManagement.Domain`：請假領域實體與 `BaseEntity`（含 `DateCreated` / `DateModified`）。  
  - `HR.LeaveManagement.Application`：CQRS（MediatR）Commands / Queries、FluentValidation 驗證、Repository / Identity / Logging 等 Contracts。  
  - `HR.LeaveManagement.Persistence`：EF Core `HrDatabaseContext`、Repositories、Migrations、`DeleteBehavior.Restrict` 設定。  
  - `HR.LeaveManagement.Infrastructure`：Serilog Logging、SendGrid Email 等技術服務。  
  - `HR.LeaveManagement.Identity`：ASP.NET Core Identity + JwtBearer + Refresh Token，實作 `IAuthService` / `IUserService`。  
  - `HR.LeaveManagement.Api`：Controllers、Exception Middleware、Swagger、Asp.Versioning v1 (`/api/v1` routes)。

- **前端（位於 `frontend/hr-leave-management-ui/`）**  
  - React + TypeScript + Vite，搭配 Tailwind CSS。  
  - 架構分為 domain / application / infrastructure / presentation：  
    - `infrastructure/apiClient` 封裝 JWT / Refresh Token 與 API 呼叫。  
    - `presentation/pages/DashboardPage` 顯示統計指標、最新請假申請與「今日重點」。  
    - `AdminLeaveRequestsPage` 與 `EmployeeLeaveRequestsPage` 分別提供管理員審核與員工檢視/取消。  
  - 角色導向導航：  
    - Admin 側邊欄：`Dashboard`、`Leave Types`、`Leave Allocations`、`Leave Requests`、`Users`。  
    - Employee 側邊欄：`My Leave Requests`。

---

### 🌐 安全基礎設施（Secure Infrastructure via Docker Compose & Nginx）

- **Docker Compose（`docker-compose.yml`）**  
  - `reverse_proxy`：Nginx 容器，負責：  
    - 服務前端 `dist` 靜態檔案（`./frontend/hr-leave-management-ui/dist`）。  
    - 反向代理 `/api` 流量到 `api` 容器。  
    - 掛載 `./certs/nginx` 提供 SSL/TLS 憑證，對外開放 HTTP / HTTPS 入口。  
  - `api`：封裝 `HR.LeaveManagement.Api` 的 ASP.NET Core Web API 容器，掛載 Logs 與 Images 資料夾。  
  - `mssql`：SQL Server 容器，掛載資料 / 備份 / Log / secrets，透過 `.env` 中的 `MSSQL_*` 變數設定。

- **Nginx Reverse Proxy with HTTPS**  
  - 使用 `conf/nginx` 中的 `nginx.conf` 與子設定檔，設定：  
    - HTTPS 端點（TLS 終結），以模擬 Production 中的 Gateway / Load Balancer。  
    - 對前端與後端 API 的路由與 Header 處理。  
  - Swagger UI 透過 Nginx 暴露為：`https://localhost/swagger`，方便 Demo 與測試。

---

### 📊 進階 Dashboard 與 Role-based UX

- **Dashboard 指標（`DashboardPage.tsx`）**  
  - 總請假申請數：`totalRequests = requests.length`。  
  - **待審核請假數**：`pendingRequests = requests.filter(r => r.approved === null && !r.cancelled).length`（只計入未取消且 Approved 為 null 的申請）。  
  - 核准率：`Approved / Total` 以百分比顯示。  
  - 最新 5 筆請假申請列表，依 `DateRequested` 由新到舊排序。

- **Admin Leave Requests 審核頁**  
  - 一次列出所有員工的請假申請，包含：員工 ID、假別、期間、申請日期、狀態。  
  - 僅對 `!cancelled && approved === null` 的申請顯示「核准 / 駁回」按鈕，其餘僅顯示狀態。  
  - 審核操作會呼叫後端 `PUT /api/v1/LeaveRequests/{id}/approve`，觸發 Cumulative Balance Validation 與 `UsedDays` 更新。

- **Employee My Leave Requests 頁**  
  - 員工僅能看到自己的申請記錄（由後端 `GetLeaveRequestListQuery` 依 UserId 過濾）。  
  - 對 `!cancelled && approved === null` 的申請提供「取消申請」按鈕，呼叫 `PUT /api/v1/LeaveRequests/{id}/cancel`。  
  - 已取消或已審核的申請不再允許前端編輯，避免狀態混亂。

#### Admin UI 畫面示意

![Admin Dashboard](docs/images/admin/dashboard.png)  
![Admin Leave Requests](docs/images/admin/leaveRequests.png)

#### Employee UI 畫面示意

![Employee Leave Requests](docs/images/employee/leaveRequestList.png)

---

### 🚀 執行方式（Docker Demo / 本機開發）

- **Docker Demo（推薦面試展示）**  
  1. 在 `frontend/hr-leave-management-ui` 執行：  
     - `npm install`  
     - `npm run build`  
  2. 準備 `.env` 檔（或指定 `ENV_FILE_PATH`），至少設定：  
     - `ASPNETCORE_ENVIRONMENT=Development`  
     - 各服務對外 Port（`NGINX_HTTP_OUTER_PORT`、`NGINX_HTTPS_OUTER_PORT`、`API_OUTER_PORT`、`MSSQL_OUTER_PORT`）  
     - MSSQL 連線與密碼設定。  
  3. 在專案根目錄執行：  

     ```bash
     docker-compose up -d --build
     ```

  4. 在瀏覽器開啟：  
     - 前端入口：`https://localhost`（實際 Port 依 `.env` 設定）。  
     - Swagger：`https://localhost/swagger`。

- **本機開發模式（不使用 Docker）**  
  - 後端：在 `api/` 以 Visual Studio / Rider 開啟 solution，將 `HR.LeaveManagement.Api` 設為啟始專案，使用 `dotnet ef database update` 建立資料庫（指令見 `docs/command/DB-Migrations.txt`）。  
  - 前端：在 `frontend/hr-leave-management-ui` 執行：  

    ```bash
    npm install
    npm run dev
    ```  

    並在 `.env` 中設定 `VITE_API_URL` 指向後端 URL（例如 `https://localhost:7133`）。

