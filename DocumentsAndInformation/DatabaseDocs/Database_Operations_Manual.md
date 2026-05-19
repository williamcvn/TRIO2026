# TRIO2026 資料庫維運手冊

> **文件編號**: TRIO2026-DB-005  
> **撰寫**: Office of William  
> **日期**: 2026-04-28  
> **版本**: 1.0  
> **適用對象**: 系統管理員、DevOps 工程師  

---

## 一、資料庫檔案位置

```
D:\TRIO2026\Database\
├── trio240plus_config.db     4,096 bytes (含 Seed Data)
├── trio240plus_main.db       4,096 bytes (含 Seed Data)
├── trio240plus_data.db       4,096 bytes (空表)
├── trio240plus_log.db        4,096 bytes (空表)
├── *.db-wal                  WAL 日誌（運行時自動產生）
└── *.db-shm                  共享記憶體（運行時自動產生）
```

---

## 二、備份策略

### 2.1 建議頻率

| 資料庫 | 備份頻率 | 原因 |
|--------|---------|------|
| config.db | 設定變更時 | 低頻寫入，配置變更才需備份 |
| main.db | 每日 | 流程/帳號異動需追蹤 |
| data.db | 每次運行後 | 檢測數據不可遺失 |
| log.db | 每週 | 可定期歸檔舊日誌 |

### 2.2 備份方式

**SQLite 安全備份（推薦）**:
```python
import sqlite3
src = sqlite3.connect('trio240plus_data.db')
dst = sqlite3.connect('backup_20260428.db')
src.backup(dst)
dst.close()
src.close()
```

**注意**: 不要在系統運行時直接複製 .db 檔案，必須使用 `backup()` API 或先執行 `PRAGMA wal_checkpoint(TRUNCATE)` 確保 WAL 已合併。

### 2.3 WAL 合併

若需要搬移或備份 .db，先執行：
```sql
PRAGMA wal_checkpoint(TRUNCATE);
```
此操作會將 WAL 日誌合併回主資料庫，使 .db 成為自包含檔案。

---

## 三、重建流程

若需從零重建資料庫（例如部署到新機器）：

```bash
# 1. 清除舊檔案
del D:\TRIO2026\Database\*.db
del D:\TRIO2026\Database\*.db-wal
del D:\TRIO2026\Database\*.db-shm

# 2. 重建表結構 + C# Seed Data
dotnet run --project tools\DbInitializer

# 3. 匯入 SystemConfig (需要存取舊系統 config/ 目錄)
python tools\import_system_config.py

# 4. 匯入 FlowDefinition + FlowStep
python tools\import_flow_definitions.py

# 5. 驗證
python tools\verify_schema.py
```

---

## 四、常用查詢

### 4.1 查看 SystemConfig

```sql
-- 列出所有分類及筆數
SELECT Category, COUNT(*) AS cnt
FROM SystemConfig
GROUP BY Category
ORDER BY cnt DESC;

-- 查詢特定分類的配置
SELECT Key, Value, DataType
FROM SystemConfig
WHERE Category = 'motor'
ORDER BY Key;

-- 查詢特定參數
SELECT Value FROM SystemConfig
WHERE Category = 'motor' AND Key = '[MT12].lead';
```

### 4.2 查看流程步驟

```sql
-- 列出所有流程及步驟數
SELECT fd.FlowName, fd.TotalSteps, COUNT(fs.Id) AS ActualSteps
FROM FlowDefinition fd
LEFT JOIN FlowStep fs ON fs.FlowDefinitionId = fd.Id
GROUP BY fd.Id;

-- 查看特定流程的前 10 步
SELECT fs.StepOrder, cd.Name AS CommandName, fs.Arg0, fs.Arg1, fs.Arg2, fs.GroupName
FROM FlowStep fs
JOIN FlowDefinition fd ON fs.FlowDefinitionId = fd.Id
LEFT JOIN CommandDefinition cd ON cd.Id = fs.CommandId
WHERE fd.FlowName = 'Opti_2'
ORDER BY fs.StepOrder
LIMIT 10;
```

> **注意**: 上面的跨庫 JOIN 無法直接執行（FlowStep 在 main.db，CommandDefinition 在 config.db），需使用 `ATTACH DATABASE` 語法。

### 4.3 ATTACH 跨庫查詢

```sql
-- 在 main.db 中附加 config.db
ATTACH DATABASE 'trio240plus_config.db' AS config;

-- 現在可以跨庫 JOIN
SELECT fs.StepOrder, config.CommandDefinition.Name, fs.Arg0
FROM FlowStep fs
JOIN config.CommandDefinition ON config.CommandDefinition.Id = fs.CommandId
WHERE fs.FlowDefinitionId = 7
ORDER BY fs.StepOrder;

DETACH DATABASE config;
```

---

## 五、效能監控

```sql
-- 檢查 WAL 大小
PRAGMA wal_checkpoint;

-- 表的頁面數量（估算資料量）
SELECT name, (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=m.name) 
FROM sqlite_master m WHERE type='table';

-- 完整性檢查
PRAGMA integrity_check;

-- 索引使用統計（需開啟）
PRAGMA compile_options;
```

---

## 六、日誌清理

建議每季度歸檔 log.db 中超過 90 天的日誌：

```sql
-- 備份舊日誌
INSERT INTO archive_operation_log SELECT * FROM OperationLog
WHERE Timestamp < datetime('now', '-90 days');

-- 清除已備份的記錄
DELETE FROM OperationLog
WHERE Timestamp < datetime('now', '-90 days');

-- 回收空間
VACUUM;
```

---

*文件結束*
