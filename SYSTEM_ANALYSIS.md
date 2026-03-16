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

- **穩定性：業務層級刪除驗證避免孤兒資料（Orphan Records）**  
  - 在 Application 層的 Handler 中，針對刪除操作（例如 LeaveAllocations）加入業務驗證，檢查是否存在關聯的 LeaveRequests。  
  - 若檢查到已存在請假紀錄，會拋出 `BadRequestException` 並由全域 Exception Middleware 統一處理，阻止違反業務規則的刪除行為。  
  - 此設計有效避免孤兒資料與邏輯不一致情況，強化系統在多步驟業務流程中的一致性。

### 系統缺點與改進空間（Weaknesses & Future Improvements）

- **資料一致性：強化資料庫層約束與刪除策略**  
  - 目前資料一致性主要由 Application 層負責檢查，雖然彈性高，但若日後有其他入口繞過 Handler（例如批次腳本或外部整合）仍有風險。  
  - 建議後續引入 **Database Level Constraints**：  
    - 在適當位置建立外鍵約束（Foreign Keys），於資料庫層強化關聯完整性。  
    - 針對關鍵實體（如 LeaveAllocations、LeaveRequests）考慮使用 `DeleteBehavior.Restrict` 防止誤刪。  
  - 同時可評估導入 **Soft Delete（軟刪除）** 機制：  
    - 以 `IsDeleted` 或 `DeletedAt` 欄位標記資料邏輯刪除，保留歷史紀錄以利稽核與追蹤。  
    - 在 Repository/Query 層統一過濾已刪除資料，避免影響現有商業邏輯。

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

- **監控與可觀測性：建置健康檢查與集中式日誌**  
  - 目前缺乏系統層級的健康檢查端點與統一監控，對於營運與故障排除不夠友善。  
  - 建議後續導入：  
    - **ASP.NET Core Health Checks**：暴露應用健康狀態（DB 連線、外部服務依賴等），並整合到 Kubernetes / Docker Swarm 或負載平衡器的探針中。  
    - **ELK / OpenSearch Stack 或其他集中式日誌方案**：將 API 與背景服務的日誌彙整到單一平台，支援全文檢索、告警與儀表板。  
    - 進一步可導入 Application Performance Monitoring（如 Prometheus + Grafana、Application Insights），追蹤關鍵交易延遲與錯誤率。

---

綜合而言，HR.LeaveManagement 已具備良好的分層架構與開發實務，適合作為長期演進的基礎。後續若能依上述方向逐步補強資料一致性、效能、資安與可觀測性，將能更從容地支援組織成長與需求變化，並降低營運風險。

