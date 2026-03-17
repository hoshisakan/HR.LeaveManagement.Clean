## HR.LeaveManagement 系統設計檢視（併發 / 配額 / 一致性 / 安全 / 狀態機）

本文件根據目前 HR.LeaveManagement（Clean Architecture + ASP.NET Core + React）的實作，從真實 HR 業務角度檢視幾個關鍵面向，著重於「不合理的設計」與「尚未覆蓋的 edge cases」，不包含具體實作方案。

---

### 1. Concurrency（併發 / Race Condition）

- **現況觀察**
  - 資料存取以 EF Core + Repository Pattern 為主，未見 RowVersion / Concurrency Token 欄位，也未在 `SaveChangesAsync` 對 `DbUpdateConcurrencyException` 做特殊處理。
  - 多數 Handler 的流程為：`GetById` → 修改屬性（如 `Approved` / `Cancelled`）→ `UpdateAsync` → `SaveChangesAsync`，缺乏「狀態轉換守門（guard）」。

- **潛在問題**
  - **多管理員同時審核同一筆**：兩人同時讀到 `Approved == null`，分別設為 `true` / `false`，最後寫入者覆蓋前者，沒有任何衝突偵測或提示。
  - **員工在審核瞬間取消申請**：Admin 在 `Cancelled == false && Approved == null` 狀態下準備核准，員工幾乎同時呼叫 Cancel 將 `Cancelled = true`；若 Handler 不再檢查當前狀態，可能出現「已核准且已取消」的矛盾終態。

- **評估**
  - 存在明顯 Race Condition，缺乏併發保護與嚴格的狀態轉換檢查，對真實 HR 流程而言風險偏高。

---

### 2. Leave Balance（配額扣除時機與不足處理）

- **現況觀察**
  - `LeaveAllocation`（年度配額）與 `LeaveRequest` 分離；Create LeaveRequest 時並未看到立即調整配額的邏輯。
  - 實作風格較接近「核准時才實際扣除」的模式（而非申請時預扣）。

- **潛在問題**
  - 若僅在核准時扣除配額，且 Approval Handler 沒有再次檢查「目前剩餘天數是否足夠」，則：
    - 員工可以同一假別連續送出多筆 Pending 申請，總天數遠超過年度配額。
    - 管理員逐筆核准時，可能在不知情的情況下讓配額出現負數或超額使用。
  - 目前看不到對「已核准 + 申請中」合計天數的防護邏輯，也沒有在核准瞬間重新評估餘額的機制。

- **評估**
  - 在真實 HR 場景中，無論選擇「申請時預扣」或「核准時扣」，核准當下都應重新驗證配額是否足夠；現有實作在這點上存在缺口。

---

### 3. Data Consistency（資料一致性：LeaveType 刪除與關聯）

- **現況觀察**
  - 資料庫關聯設定（依 Migration / ModelSnapshot）：
    - `LeaveAllocations.LeaveTypeId` → `LeaveTypes.Id`：`OnDelete(DeleteBehavior.Cascade)`
    - `LeaveRequests.LeaveTypeId` → `LeaveTypes.Id`：`OnDelete(DeleteBehavior.Cascade)`
  - 刪除 LeaveType 會連帶刪除所有相同假別的 `LeaveAllocation` 與 `LeaveRequest`。

- **潛在問題**
  - HR 實務上，歷史請假紀錄與配額紀錄多為稽核與法遵資料，不應因為「停用或改名某假別」而整批刪除。
  - 目前設定下，一個誤操作（刪除 LeaveType）即可消除整個假別的歷史申請與配額，風險極高。

- **評估**
  - 技術上雖「一致」，但從 HR 業務角度屬於不合理設計。更常見做法是：
    - 使用 `DeleteBehavior.Restrict` 禁止刪除仍被引用的 LeaveType，或
    - 改為軟刪（`IsActive` / `IsArchived`）以保留所有歷史關聯資料。

---

### 4. Security（安全性：員工是否能操作他人 LeaveRequest）

- **現況觀察**
  - Controller 端從 JWT 取得 `uid`，並在 Create / Update / Cancel 時，將 `RequestingEmployeeId` / `UserId` 強制設為該值，而非相信前端傳入的 Id。
  - Query Handler（列表／明細）會根據 `IUserService.IsEmployee(userId)` 決定是「只看自己的」還是「可看全部」。

- **潛在問題**
  - 安全關鍵在於 Application Handler 是否再加一層檢查：
    - `UpdateLeaveRequestCommandHandler` / `CancelLeaveRequestCommandHandler` 是否驗證「目標 LeaveRequest 的 `RequestingEmployeeId` 與呼叫者一致，或呼叫者為 Admin」。
  - 若 Handler 僅依賴 Controller 預先塞入的欄位而不再驗證，雖然在目前單一 API 入口下仍相對安全，但：
    - 未來若新增其他入口（Batch、外部整合、另一路 API）可能因未複製相同邏輯而出現漏洞。

- **評估**
  - 目前設計方向正確（不信任前端 Id，改用 JWT Claim），但從「防禦縱深」觀點，Handler 層仍應進行擁有權驗證，避免未來擴充時產生繞過風險。

---

### 5. Status Workflow（狀態機設計）

- **現況觀察**
  - `LeaveRequest` 狀態欄位：
    - `Approved: bool?`（`true/false/null`）
    - `Cancelled: bool`
  - 前端的狀態對應：
    - `Cancelled == true` → 已取消
    - `Approved == true` → Approved
    - `Approved == false` → Rejected
    - `Approved == null && !Cancelled` → Pending

- **能覆蓋的情境**
  - 基本流程：申請 → 審核（核准/駁回）→ 取消。

- **不足或模糊的情境**
  - **已銷假 / 已執行完畢**：缺乏區別「已核准」與「實際已休完」的明確狀態，難以與出勤打卡、薪資結算對帳。
  - **已過期未休**：申請期間已過去但員工未實際請假，系統只能透過日期推斷，沒有明確的過期狀態。
  - **部分核准**：同一申請若只核其中數日，現有模型無法表達「部分期間核准」這種複雜情境。
  - **退回補件 / 改期**：若 HR 流程需要「退件再送」或「要求修改日期」，現有三值邏輯很難表達。

- **評估**
  - 對中小型、流程簡化的 HR 系統而言，目前狀態欄位可以滿足基本需求。
  - 若系統需支援更細緻的 HR 流程（銷假、過期處理、部分核准等），現有狀態機設計偏單薄，未來擴充時可能需要結構性重構。

---

### 總結

- **Concurrency**：目前缺乏併發控制與嚴格的狀態轉換檢查，多管理員/員工同時操作時存在 Race Condition，可能導致互相覆寫或矛盾狀態。
- **Leave Balance**：採「核准時扣除配額」的傾向，但 Approval 時缺乏重新驗證餘額的機制，無法防止 Pending 申請累積導致超額核准。
- **Data Consistency**：`LeaveType` 刪除使用 Cascade 連帶刪除所有歷史資料，從 HR 實務與法遵角度並不合理，建議改為限制刪除或軟刪。
- **Security**：Controller 端已正確使用 JWT Claim 控制基本權限，但 Application Handler 層仍可加上擁有權驗證以強化防護深度。
- **Status Workflow**：`Approved? + Cancelled` 足以覆蓋簡化版流程，但對「已銷假、已過期、部分核准、退回補件」等實務情境支援不足，是未來系統演進時可優先檢討的設計點。

