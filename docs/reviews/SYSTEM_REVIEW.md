## HR.LeaveManagement 系統設計檢視（併發 / 配額 / 一致性 / 安全 / 狀態機）

本文件根據目前 HR.LeaveManagement（Clean Architecture + ASP.NET Core + React）的實作，從真實 HR 業務角度檢視幾個關鍵面向，並標記每一項議題的最新狀態（[STATUS]），以展示從風險識別到解決方案落地的完整演進。

---

### 1. Concurrency（併發 / Race Condition） [STATUS: OPEN]

- **現況觀察**
  - 資料存取以 EF Core + Repository Pattern 為主，未見 RowVersion / Concurrency Token 欄位，也未在 `SaveChangesAsync` 對 `DbUpdateConcurrencyException` 做特殊處理。
  - 多數 Handler 的流程為：`GetById` → 修改屬性（如 `Approved` / `Cancelled`）→ `UpdateAsync` → `SaveChangesAsync`，缺乏「狀態轉換守門（guard）」。

- **潛在問題**
  - **多管理員同時審核同一筆**：兩人同時讀到 `Approved == null`，分別設為 `true` / `false`，最後寫入者覆蓋前者，沒有任何衝突偵測或提示。
  - **員工在審核瞬間取消申請**：Admin 在 `Cancelled == false && Approved == null` 狀態下準備核准，員工幾乎同時呼叫 Cancel 將 `Cancelled = true`；若 Handler 不再檢查當前狀態，可能出現「已核准且已取消」的矛盾終態。

- **評估**
  - 存在明顯 Race Condition，缺乏併發保護與嚴格的狀態轉換檢查，對真實 HR 流程而言風險偏高。這部分刻意保留為未來可深挖的技術主題（例如導入樂觀鎖、RowVersion 欄位、條件式更新與重試機制）。

---

### 2. Leave Balance（配額扣除時機與不足處理） [STATUS: RESOLVED]

- **最新實作：Cumulative Balance Validation**  
  - 每個年度配額（`LeaveAllocation`）除了 `NumberOfDays`（年度總配額）外，新增 `UsedDays` 欄位，用來紀錄「已核准且未取消」的累計請假天數。  
  - 在 `ChangeLeaveRequestApprovalCommandHandler` 中，當管理員將一筆請假從 Pending/Rejected 改為 Approved 時，系統會：  
    - 計算本次請假天數 `daysRequested = (EndDate.Date - StartDate.Date).TotalDays + 1`。  
    - 讀取該員工、該假別、該年度的 `LeaveAllocation`。  
    - 檢查 `(allocation.UsedDays + daysRequested) <= allocation.NumberOfDays`；若不成立，則視為 **Over-allocation**，丟出 `BadRequestException("Insufficient leave balance for this request.")`，阻止核准與任何狀態更新。  
  - 若已核准的請假後續被改為未核准或取消，Handler 會同步將對應天數自 `UsedDays` 扣回，維持年度餘額精確。

- **評估（狀態：RESOLVED）**  
  - 透過「已核准總天數 + 新請求天數」的累計驗證，系統能保證即使存在多筆 Pending 與多位管理員逐筆審核，年度配額也不會在無意間被核准超標，Leave Balance 風險已由程式實作層級收斂。

---

### 3. Data Consistency（資料一致性：LeaveType 刪除與關聯） [STATUS: RESOLVED]

- **最新實作：Data Safety & Referential Integrity**  
  - 在 `HrDatabaseContext` 中，針對兩組關聯明確設定 `DeleteBehavior.Restrict`：  
    - `LeaveRequest.LeaveTypeId` → `LeaveType.Id`  
    - `LeaveAllocation.LeaveTypeId` → `LeaveType.Id`  
  - 實際效果為：  
    - 一旦某個 `LeaveType` 已被任何 `LeaveRequest` 或 `LeaveAllocation` 參考，嘗試刪除該假別會觸發資料庫層級的 FK 衝突錯誤，刪除動作被拒絕。  
    - 歷史請假紀錄與年度配額資料不會因為「停用或改名假別」而被級聯刪除，Audit Trail 得以完整保留。

- **評估（狀態：RESOLVED）**  
  - 透過將 Cascade 刪除改為 Restrict，Referential Integrity 被交由資料庫強制執行，避免僅依賴應用層檢查。  
  - HR 報表與稽核資料在長期維運中更可靠，Data Safety 議題已獲得實作層級的解決。

---

### 4. Security（安全性：員工是否能操作他人 LeaveRequest） [STATUS: OPEN]

- **現況觀察**
  - Controller 端從 JWT 取得 `uid`，並在 Create / Update / Cancel 時，將 `RequestingEmployeeId` / `UserId` 強制設為該值，而非相信前端傳入的 Id。  
  - Query Handler（列表／明細）會根據 `IUserService.IsEmployee(userId)` 決定是「只看自己的」還是「可看全部」。

- **潛在問題**
  - 安全關鍵在於 Application Handler 是否再加一層檢查：  
    - `UpdateLeaveRequestCommandHandler` / `CancelLeaveRequestCommandHandler` 是否驗證「目標 LeaveRequest 的 `RequestingEmployeeId` 與呼叫者一致，或呼叫者為 Admin」。  
  - 若 Handler 僅依賴 Controller 預先塞入的欄位而不再驗證，雖然在目前單一 API 入口下仍相對安全，但：  
    - 未來若新增其他入口（Batch、外部整合、另一路 API）可能因未複製相同邏輯而出現漏洞。

- **評估**
  - 目前設計方向正確（不信任前端 Id，改用 JWT Claim），但從「防禦縱深」觀點，Handler 層仍應進行擁有權驗證，避免未來擴充時產生繞過風險。狀態暫維持 [OPEN]，可在後續版本納入更完整的授權策略。

---

### 5. Status Workflow（狀態機設計） [STATUS: OPEN]

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
  - 若系統需支援更細緻的 HR 流程（銷假、過期處理、部分核准等），現有狀態機設計偏單薄，未來擴充時可能需要結構性重構，因此狀態維持 [OPEN] 作為後續演進主題。

---

### 總結（狀態一覽）

- **Concurrency [STATUS: OPEN]**：目前缺乏併發控制與嚴格的狀態轉換檢查，多管理員/員工同時操作時仍存在 Race Condition，可作為後續導入樂觀鎖、RowVersion 或條件式更新的技術深潛主題。  
- **Leave Balance / Over-allocation [STATUS: RESOLVED]**：已透過 `UsedDays` 累計與 Cumulative Balance Validation 實作「Already Approved + New Request <= Total Allocation」，避免年度配額被多筆請求累計超標。  
- **Data Consistency / Referential Integrity [STATUS: RESOLVED]**：`LeaveType` 關聯改用 `DeleteBehavior.Restrict`，禁止刪除仍被參考的假別，確保持久且一致的 Audit Trail。  
- **Security [STATUS: OPEN]**：Controller 端已正確使用 JWT Claim 控制基本權限，但 Application Handler 層未來仍可加上擁有權驗證與更細膩的授權規則，以強化防禦縱深。  
- **Status Workflow [STATUS: OPEN]**：`Approved? + Cancelled` 模型足以支援簡化流程，但對「已銷假、已過期、部分核准、退回補件」等進階情境仍需後續演進與可能的狀態機重構。  

