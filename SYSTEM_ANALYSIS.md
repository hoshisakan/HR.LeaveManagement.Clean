## HR.LeaveManagement 系統優缺點分析報告

本報告從架構、開發維運與穩定性等面向，對現行 HR.LeaveManagement 系統進行技術盤點，目的在於為下一階段的系統演進與優化提供決策依據與參考方向。

### 系統優點（Strengths）

- **架構面：Clean Architecture 與 CQRS 帶來高內聚、低耦合**  
  - 採用 Clean Architecture 分層（Domain / Application / Persistence / Api / UI），讓業務規則與技術細節有明確邊界，降低跨層依賴。  
  - Command / Query 分離（CQRS）讓讀寫邏輯各自演進：  
    - Commands 聚焦在狀態變更與交易一致性。  
    - Queries 聚焦在讀取與投影模型（DTO），方便調整對前端的輸出格式。  
  - 這種設計提升可測試性與可維護性，也為未來拆分微服務或引入訊息佇列奠定良好基礎。

- **開發面：MediatR 與 AutoMapper 減少樣板程式碼（Boilerplate）**  
  - 使用 MediatR 實作 Use Case Handler，Controller 僅負責將 HTTP 請求轉交 `Mediator.Send`，大幅降低 Controller 中的商業邏輯與重複程式碼。  
  - AutoMapper 將 Domain、DTO、Command/Query Model 之間的映射集中管理，避免手動對欄位一一指派：  
    - 降低 Mapping 錯誤風險。  
    - 讓新增／調整欄位時的修改點明確集中。  
  - 整體而言，開發人員可以把心力放在業務流程與規則，而非反覆撰寫資料搬運程式。

- **維運面：Docker 容器化與 Nginx Resolver 優化**  
  - 系統以 Docker 進行容器化封裝，將 API、前端與依賴服務（如資料庫、反向代理）標準化，降低環境差異帶來的問題。  
  - Nginx 作為反向代理與靜態檔案伺服器，結合 Resolver 設定，解決容器啟動順序與 DNS 解析時序問題，提升系統在多容器環境啟動時的穩定性。  
  - 對應 CI/CD 流程時，可快速啟動多組環境（Dev / QA / Staging / Prod）並確保行為一致。

- **穩定性：業務層級刪除驗證與配額控管**  
  - 在 Application 層的 Handler 中，針對刪除操作（例如 LeaveAllocations）加入業務驗證，檢查是否存在關聯的 LeaveRequests。  
  - 若檢查到已存在請假紀錄，會拋出 `BadRequestException` 並由全域 Exception Middleware 統一處理，阻止違反業務規則的刪除行為，避免孤兒資料與邏輯不一致。  
  - 在請假核准（Leave Approval）流程中，系統額外實作了 **Cumulative Balance Validation** 以防止 Over-allocation：  
    - 每個年度配額（LeaveAllocation）除了 `NumberOfDays`（總配額）外，透過 `UsedDays` 累積「已核准且未取消」的請假天數。  
    - 當管理員將一筆請假從 Pending/Rejected 改為 Approved 時，Handler 會計算本次請假天數，並檢查 `(UsedDays + daysRequested) <= NumberOfDays`；若不成立，則視為配額超用，直接丟出 `BadRequestException("Insufficient leave balance for this request.")`，確保資料與配額帳務精確一致。  
    - 若將已核准的請假改為未核准或取消，系統會同步扣回對應的 `UsedDays`，維持同一員工在同一年度下的配額餘額正確性。

### 系統缺點與改進空間（Weaknesses & Future Improvements）

- **資料一致性與 Referential Integrity：DB 約束策略已啟用，但仍有擴充空間**  
  - 系統目前已在 Persistence 層對 `LeaveRequest.LeaveTypeId` 與 `LeaveAllocation.LeaveTypeId` 設定 `DeleteBehavior.Restrict`，強化 Referential Integrity：  
    - 一旦某個 `LeaveType` 已被歷史請假或配額紀錄參考，刪除該假別會被資料庫拒絕，保護 Audit Trail 不被意外清除。  
  - 未來仍可針對其他關鍵關聯補充更多資料庫層級約束（例如對 Identity 或其他業務實體新增 FK / Unique Index），並評估是否在部分情境導入 Soft Delete（軟刪除）以簡化商業邏輯對歷史資料的處理。  
  - 此外，可考慮在報表或歷史查詢場景中提供「已封存假別」的視覺標示，讓使用者理解該假別已無法再被指派，但仍保留於 Audit Trail 中供追溯。

- **效能優化：針對高頻讀取導入 Dapper 或專用讀取模型**  
  - 目前讀寫均大量依賴 Entity Framework Core，對一般 CRUD 來說足夠，但在高頻查詢場景（例如儀表板、統計報表）可能產生額外 ORM 負擔。  
  - 未來可在 CQRS 的 Query 端引入 **Dapper** 或原生 SQL：  
    - 對只讀場景使用輕量級 ORM，減少不必要的變更追蹤與物件圖建構。  
    - 針對跨多表的彙總查詢設計專門的 Read Model，優化載入時間與資料庫負載。  
  - 同時可配合快取策略（Memory Cache / Distributed Cache）優化熱門查詢。

- **安全性：Token 生命週期管理與進階防護**  
  - 系統已實作 **JWT Access Token + Refresh Token** 機制：  
    - 使用 `TokenRepository` 產生簽章正確的 JWT，並為每個 JWT 建立對應的 Refresh Token（含 `JwtId`、`UserId`、到期日與使用／撤銷狀態）。  
    - `AuthService` 與 `RefreshTokenCommandHandler` 會在刷新流程中驗證 Refresh Token 是否已撤銷、已使用或過期，並在成功後產生新的 Access Token 與 Refresh Token，同時將舊 Token 標記為已使用與已撤銷。  
  - 後續仍可強化的方向包括：  
    - 引入更嚴格的 Refresh Token 旋轉與異常偵測策略（如短時間內多次嘗試、IP / 裝置指紋比對）。  
    - 規劃密碼變更、角色調整或安全事件發生時的 Token 全面作廢與強制重新登入流程。  

- **例外處理與 Business Rule Violations：可進一步結構化錯誤回應**  
  - 在 Application 層，對多數 Business Rule Violations（例如配額不足的 Over-allocation、刪除仍有關聯資料、非法狀態轉換等）統一透過自訂例外類型（`BadRequestException`、`NotFoundException` 等）拋出。  
  - 這些例外由全域 Exception Middleware 攔截並轉換為結構化的 API 回應（適當的 HTTP 狀態碼與訊息），將領域層的錯誤語意清楚地暴露給前端與整合系統。  
  - 目前錯誤格式已足以區分「輸入資料問題 / 業務規則違反」與「系統性錯誤」，未來仍可導入標準化錯誤模型（含錯誤碼、欄位名稱與本地化訊息），以便在前端更精準地顯示 Validation 結果，並在監控系統中針對特定 Business Rule Violations 做統計與告警。

---

綜合而言，HR.LeaveManagement 已具備良好的分層架構與開發實務，適合作為長期演進的基礎。後續若能依上述方向逐步補強資料一致性、效能、資安與可觀測性，將能更從容地支援組織成長與需求變化，並降低營運風險。

