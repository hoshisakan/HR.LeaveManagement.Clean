## HR.LeaveManagement 系統架構說明（Data Integrity / Stateful Logic / Secure Infra）

本文件聚焦於三個架構支柱，說明此專案如何在 **資料完整性（Data Integrity）**、**狀態化業務邏輯（Stateful Business Logic）** 與 **安全基礎設施（Secure Infrastructure）** 上做出專業且可擴充的設計決策。

---

### 1. 🛡️ Data Integrity & Audit Protection（Persistence Layer）

#### 1.1 Referential Integrity：從 Cascade 到 Restrict

過去常見的做法是將外鍵設成 `DeleteBehavior.Cascade`，刪除主表時自動級聯刪除相關資料；然而在 HR / 財務等領域，歷史紀錄是 Audit Trail 的一部分，**不應輕易物理刪除**。

在本系統中，Persistence 層的 `HrDatabaseContext` 明確針對下列關聯設定為 `DeleteBehavior.Restrict`：

- `LeaveRequest.LeaveTypeId` → `LeaveType.Id`  
- `LeaveAllocation.LeaveTypeId` → `LeaveType.Id`

效果如下：

- 若某個 `LeaveType` 已被任何 `LeaveRequest` 或 `LeaveAllocation` 參考，**刪除該假別會被資料庫層的 FK 約束拒絕**。  
- 管理員必須先處理或封存關聯資料，才能對假別做進一步處理，避免誤刪歷史請假紀錄。

這項決策將 Referencial Integrity 的責任交由資料庫強制執行，而非僅依賴應用程式邏輯，大幅降低因程式 Bug 或維運操作失誤而清空關聯資料的風險。

#### 1.2 Audit Trail Preservation：為何避免物理刪除？

在真實 HR 場景中，以下需求十分常見：

- 稽核或檢查期間需要回顧「某年度員工實際請了哪些假」。  
- 需要針對爭議案件調閱歷史申請與當時審核決策。  
- 需要長期維護 KPI / 使用率等統計資料。

因此：

- **LeaveRequests** 與 **LeaveAllocations** 被視為交易紀錄（Transactional Records），一旦產生，應該長期保留。  
- 即便假別（LeaveType）已不再提供（例如政策改版），我們也不應刪除過去所有使用該假別的歷史紀錄。

採用 `DeleteBehavior.Restrict` 之後：

- **任何嘗試刪除仍被參考的 LeaveType 都會立即失敗**，並由上層 Exception Middleware 將錯誤轉為清楚的 API 回應。  
- 這種設計使系統具備高度的 **Audit Trail 保護能力**，滿足企業與法遵對歷史資料可追溯性的要求。

---

### 2. ⚡ Stateful Business Logic（Domain & Application Layers）

此系統不僅儲存靜態資料，更在領域層與應用層中實作了可被稽核與驗證的**狀態化業務邏輯**，尤其體現在「請假配額與核准流程」上。

#### 2.1 Resource Allocation：UsedDays 與即時配額追蹤

在 `HR.LeaveManagement.Domain` 的 `LeaveAllocation` 實體中，除了年度配額 `NumberOfDays` 外，新增了：

- `UsedDays`：整年度目前已核准且未取消的請假天數累計。

這個欄位讓系統能直接反映「資源使用狀態（Resource Allocation）」：

- `NumberOfDays`：總可用額度。  
- `UsedDays`：已消耗額度。  
- `RemainingDays = NumberOfDays - UsedDays`：可推導出即時剩餘額度。

#### 2.2 Cumulative Balance Validation：避免 Over-allocation

在 Application 層中，`ChangeLeaveRequestApprovalCommandHandler` 負責處理請假核准的 Command。  
當管理員嘗試將某筆請假從 Pending/Rejected 改為 Approved 時，Handler 會進行 **Cross-Entity Validation**：

1. 取得目標 `LeaveRequest`，並計算本次請假天數 `daysRequested`：  
   - `daysRequested = (EndDate.Date - StartDate.Date).TotalDays + 1`
2. 從 `ILeaveAllocationRepository` 取得該員工、該假別、該年度的 `LeaveAllocation`。  
3. 驗證：

   ```text
   allocation.UsedDays + daysRequested <= allocation.NumberOfDays
   ```

4. 若上式不成立，代表此核准會造成 **Over-allocation**，Handler 直接拋出 `BadRequestException("Insufficient leave balance for this request.")`，阻止任何狀態與配額更新。
5. 若成立，則：
   - 將 `LeaveRequest.Approved` 設為 `true`。  
   - 將 `LeaveAllocation.UsedDays += daysRequested`。

當已核准的請假被改為未核准或取消時，Handler 會在同一個流程中將對應天數自 `UsedDays` 扣回，確保年度餘額恢復正確。

> **關鍵點**：系統不只確認「單筆請求 ≤ 配額」，而是確認「**已核准總量 + 新請求量 ≤ 年度總配額**」，從領域層就消除超額使用的可能性。

#### 2.3 Atomicity：Unit of Work 與交易一致性

`HrDatabaseContext` 實作了典型的 **Unit of Work** 模式：

- 追蹤所有受影響的 `LeaveRequest` 與 `LeaveAllocation` 實體。  
- 在 `SaveChangesAsync` 時，以單一交易（Transaction）一次性提交變更。

在 Leave Approval 流程中，以下操作會在同一交易中完成：

- 更新 `LeaveRequest.Approved` 狀態。  
- 更新 `LeaveAllocation.UsedDays` 累計值。

若任何一個更新或驗證步驟發生例外：

- 整個交易回滾，保證「請假狀態」與「配額餘額」永遠一致。  
- 這符合 ACID 中的 **Atomicity** 要求，尤其在多管理員同時審核同一員工不同請假紀錄的情境下，顯得格外重要。

---

### 3. 🌐 Secure Infrastructure（Nginx Reverse Proxy & HTTPS）

最後一個支柱是透過 **Nginx + Docker Compose** 建立一個接近 Production 的安全基礎設施，讓整個系統從一開始就考慮到部署與安全性。

#### 3.1 Nginx Reverse Proxy：Gateway / Edge Role

`docker-compose.yml` 中的 `reverse_proxy` 服務扮演 Gateway 角色：

- 將使用者的所有 HTTP/HTTPS 請求集中進入 Nginx。  
- 透過 `conf/nginx/nginx.conf` 與 `conf/nginx/conf.d`：
  - 服務 React 前端編譯後的靜態資源（`/usr/share/nginx/html`）。  
  - 將 `/api` 或相關路由反向代理（Reverse Proxy）到後端 `api` 容器的 ASP.NET Core Web API。  
  - 統一設定 CORS / Header / 日誌與錯誤頁面。

這樣的設計讓 Web API 與資料庫只需暴露在 Docker Network 內，不直接對外開放，有助於隔離與防護。

#### 3.2 SSL/TLS（HTTPS）端點與 Swagger 入口

- Nginx 透過掛載 `./certs/nginx` 中的憑證與金鑰，提供正式 HTTPS 端點。  
- 這讓本機或 Demo 環境也能以 **`https://localhost`** 的形式運作，接近實際上線配置。  
- Swagger UI 經由 Reverse Proxy 暴露為：

  ```text
  https://localhost/swagger
  ```

  這對於面試展示或對第三方說明 API 能力時非常直覺。

#### 3.3 Role-based Routing & Security Posture

雖然實際角色授權主要發生在 Application 層與 Identity（JWT Claims + ASP.NET 授權），但從架構角度：

- Nginx 作為邊界層，可在需要時加入：
  - 基於路徑或網段的基本限制（例如只允許內網直接呼叫特定管理端點）。  
  - 統一的 Rate Limit / IP 白名單等安全策略。  
- 目前的設計將這層考量預留在 Infra，未來若需要更嚴格的 API Gateway 策略，可在不修改後端程式碼的情況下於 Nginx 層擴充。

---

### 總結：三大支柱如何整合

- **Data Integrity & Audit Protection**：  
  - 使用 `DeleteBehavior.Restrict` 與資料庫層級的 Referential Integrity，確保歷史紀錄與 Audit Trail 永遠不會被誤刪。  

- **Stateful Business Logic**：  
  - 透過 `UsedDays` + Cumulative Balance Validation，將 Resource Allocation 規則嵌入到 Application 層的 Command Handler 中，並以 Unit of Work 確保核准與扣點的 Atomicity。  

- **Secure Infrastructure**：  
  - 以 Nginx Reverse Proxy + HTTPS + Docker Compose 建立接近生產的部署拓樸，讓系統在一開始就具備良好的安全姿態與可運維性。

這三個支柱共同構成一套可在技術面試中深入說明的架構故事：  
從純粹的程式分層，提升到 **資料可靠性、狀態一致性與部署安全性** 三個面向的綜合考量。 

