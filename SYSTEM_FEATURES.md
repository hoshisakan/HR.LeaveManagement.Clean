## HR.LeaveManagement 系統功能報告

### 專案核心架構

- **整體架構**  
  - 採用 **Clean Architecture** 分層：`Domain`、`Application`、`Persistence`、`Api`、`UI`，透過明確的依賴方向與抽象介面降低耦合、提升可測試性。  
  - 商業邏輯集中於 `Application` 層，透過介面 (`Contracts.Persistence`) 對資料存取與基礎設施進行反轉相依，避免上層直接依賴 EF Core 或外部服務。

- **CQRS 與 MediatR**  
  - 以 **CQRS** 模式切分讀寫：  
    - **Commands**：對應資料變更操作（Create/Update/Delete），使用 `IRequest<T>` / `IRequest` 搭配各自的 `CommandHandler`。  
    - **Queries**：對應查詢操作（Get List / Get Detail），使用專用 DTO 與 Query Handler，避免暴露 Domain 實體。  
  - 透過 **MediatR** 將 Controller 與 Use Case 解耦：API 僅負責接收 HTTP 請求並轉交 `Mediator.Send(...)`，實際邏輯由 Handler 執行，促進單一職責與測試隔離。

- **Repository Pattern 與 Unit of Work**  
  - `IGenericRepository<T>` 與各功能專用 Repository（如 `ILeaveTypeRepository`、`ILeaveAllocationRepository`、`ILeaveRequestRepository`）封裝 EF Core 操作，提供一致的 CRUD 介面與領域語意查詢。  
  - `HrDatabaseContext` 扮演工作單元角色，集中管理交易與變更追蹤，並在 `SaveChangesAsync` 中自動維護 `DateCreated`、`DateModified` 等審計欄位。

### 使用者角色與權限（RBAC）

- **Admin（管理員）**  
  - 管理系統基礎資料與組織資源：  
    - 維護 **假別（Leave Types）**：新增、修改、刪除與檢視各種假別及其預設天數。  
    - 維護 **配額分配（Leave Allocations）**：為員工建立年度休假配額、批次指派及調整。  
    - 檢視與審核 **請假申請（Leave Requests）**：決定核准或拒絕，並可管理整體請假狀態。  
  - 透過 UI 與 API 僅授權存取 Admin 專屬的路由與操作，避免一般員工直接存取管理功能。

- **Employee（員工）**  
  - 僅能執行與自身相關的操作：  
    - 檢視個人配額（My Allocations）、剩餘天數與歷史請假記錄。  
    - 建立、更新或取消自己的請假申請（前提為尚未被審核或符合規則）。  
  - 系統以使用者身分識別（JWT Claim / UserId）在 Handler 與 Repository 層限制查詢範圍，實作細緻的 **Row-Level Security** 邏輯。

### 核心功能模組（Features）

- **假別管理（Leave Types）**  
  - **CRUD 運作**：  
    - 透過 `LeaveType` Commands/Queries 實作新增、修改、刪除與查詢假別。  
    - 使用對應 DTO（如 `LeaveTypeDto`）與 AutoMapper Profile 進行模型映射，確保 Domain 與 API Contract 分離。  
  - **預設值設定**：  
    - 在 `Persistence` 層的 Migrations/Configurations 中種入預設假別（如 Vacation）及 `DefaultDays`，提供系統啟動時的基準資料。

- **配額分配（Leave Allocations）**  
  - **年度配額初始化**：  
    - `CreateLeaveAllocation` Command Handler 依據假別與年度，為目標員工建立對應的 `LeaveAllocation` 記錄。  
    - 透過 `LeaveAllocationRepository.AllocationExists` / `LeaveAllocationMustExist` 等方法避免重複建立同年度同假別的配額。  
  - **配額管理**：  
    - Admin 可調整 `NumberOfDays`、`Period` 等欄位，並透過 DTO 在 UI 呈現目前配額與已用天數（由其他查詢計算）。  
    - 查詢 Handler 提供管理員視角（全部員工）與員工視角（僅個人）的列表資料。

- **請假申請（Leave Requests）**  
  - **申請流程**：  
    - Employee 透過 `CreateLeaveRequestCommand` 提交請假申請，Handler 會驗證日期區間、假別合法性與可用配額，再建立 `LeaveRequest` 實體。  
    - 系統會填入 `DateRequested` 與對應的 `LeaveTypeId`、`RequestingEmployeeId`，並可發送通知 Email。  
  - **狀態變更**：  
    - **Approved**：Admin 審核通過後，更新請假狀態並調整對應 `LeaveAllocation` 的剩餘天數。  
    - **Rejected**：Admin 駁回申請，保留原配額不變，並在 `LeaveRequest` 中記錄審核結果。  
    - **Cancelled**：員工或 Admin 可取消尚未執行的請假，系統會標記 `Cancelled = true`，必要時回補配額。  
  - **查詢與明細**：  
    - Query Handlers 提供列表與明細視圖，結合 `LeaveType` 讓 UI 能顯示假別名稱與日期範圍。

### 系統穩定性與防護（System Integrity）

- **全域異常處理（Global Exception Handling）**  
  - 在 API 專案中使用 **Exception Middleware** 攔截未處理例外，將其轉換為一致的 HTTP 回應格式（含錯誤碼、訊息與細節）。  
  - 對於自訂例外（如 `BadRequestException`、`NotFoundException`）進行分類處理，產生具語意的狀態碼（400/404 等），避免內部堆疊資訊外洩。

- **刪除驗證邏輯與資料一致性（Data Integrity）**  
  - 在各功能 Handler 中，刪除操作前會先進行業務層級驗證，避免破壞關聯資料。  
  - **特別規則：防止刪除已存有請假紀錄的配額**  
    - `DeleteLeaveAllocationCommandHandler` 在呼叫 Repository 刪除前，會透過 `ILeaveRequestRepository` 檢查是否存在與目標配額相同 `EmployeeId` 與 `LeaveTypeId` 的 `LeaveRequest`。  
    - 若存在關聯請假紀錄，Handler 會拋出 `BadRequestException("無法刪除，因為該員工已有相關的請假申請紀錄")`，由 Middleware 統一回傳至前端，確保資料庫與業務狀態保持一致。

### 效能與安全

- **認證與授權（Security - JWT & RBAC）**  
  - 採用 **JWT（JSON Web Token）** 實作身份驗證，API 透過 Bearer Token 驗證呼叫方身分。  
  - 使用 Claims（例如角色、使用者識別碼）結合 ASP.NET 授權機制與應用層邏輯，實現 Admin / Employee 的 **Role-Based Access Control**。  

- **AutoMapper DTO 映射（Performance & Maintainability）**  
  - 在 `Application.MappingProfiles` 中定義 `LeaveTypeProfile`、`LeaveAllocationProfile`、`LeaveRequestProfile` 等，集中維護 Domain ↔ DTO ↔ Command 之間的映射。  
  - 降低重複手動 Mapping 的程式碼量，減少錯誤與維護成本，同時提升查詢與寫入流程的一致性與可閱讀性。

- **Docker 容器化與 Nginx Resolver 優化（Deployment & Scalability）**  
  - 將 API 與前端 UI 透過 **Docker** 進行容器化封裝，統一依賴環境、簡化部署流程，利於在測試、預備與正式環境間遷移。  
  - 使用 **Nginx** 作為反向代理與靜態資源伺服器，並設定動態 Resolver 與快取策略，優化服務發現與跨容器 DNS 查詢，提高系統在多容器環境中的穩定性與吞吐量。  
  - 搭配健康檢查與日誌集中化（Logging）配置，有助於監控執行狀況與快速診斷問題。

