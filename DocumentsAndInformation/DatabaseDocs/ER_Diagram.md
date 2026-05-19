# TRIO2026 實體關聯圖 (ER Diagram)

> **文件編號**: TRIO2026-DB-003  
> **撰寫**: Office of William  
> **日期**: 2026-04-28  
> **版本**: 1.0  

---

## 一、四庫全域 ER 圖

```mermaid
erDiagram
    %% ====== trio240plus_config.db ======
    SystemConfig {
        INTEGER Id PK
        TEXT Category
        TEXT Key
        TEXT Value
        TEXT DataType
        TEXT Description
        TEXT ModifiedAt
        TEXT ModifiedBy
    }
    
    CommandDefinition {
        INTEGER Id PK
        TEXT Name
        INTEGER Arg0Type
        TEXT Arg0Label
        TEXT Arg0Options
        INTEGER Arg1Type
        TEXT Arg1Label
        TEXT Arg1Options
        INTEGER Arg2Type
        TEXT Arg2Label
        TEXT Arg2Options
        INTEGER Arg3Type
        TEXT Arg3Label
        TEXT Arg3Options
        INTEGER Arg4Type
        TEXT Arg4Label
        TEXT Arg4Options
        TEXT Note
        TEXT DisplayFormat
    }

    %% ====== trio240plus_main.db ======
    UserAccount {
        INTEGER Id PK
        TEXT Username
        TEXT PasswordHash
        INTEGER RoleLevel
        INTEGER IsActive
        TEXT CreatedAt
        TEXT LastLoginAt
    }

    FlowDefinition {
        INTEGER Id PK
        TEXT FlowName
        TEXT Description
        INTEGER TotalSteps
        TEXT Version
        TEXT SampleType
        INTEGER IsActive
        TEXT CreatedAt
        TEXT ModifiedAt
        TEXT ModifiedBy
    }

    FlowStep {
        INTEGER Id PK
        INTEGER FlowDefinitionId FK
        INTEGER StepOrder
        INTEGER CommandId
        INTEGER Crc
        REAL Arg0
        REAL Arg1
        REAL Arg2
        REAL Arg3
        REAL Arg4
        TEXT StringArg
        TEXT GroupName
        INTEGER GroupDepth
    }

    FlowMapping {
        INTEGER Id PK
        TEXT ProductCode
        TEXT FlowName
        INTEGER ProcessingTimeMin
        TEXT SampleVolumeUl
        TEXT SampleType
        TEXT ModuleConfig
        INTEGER EstimatedSeconds
    }

    PnidMapping {
        INTEGER Id PK
        TEXT PnidCode
        TEXT DescriptionEn
        TEXT DescriptionZh
        TEXT LinkedProductCode
    }

    %% ====== trio240plus_data.db ======
    TestRecord {
        INTEGER Id PK
        TEXT RunId
        TEXT FlowName
        TEXT ProductCode
        TEXT OperatorName
        INTEGER SampleCount
        TEXT StartTime
        TEXT EndTime
        TEXT Status
        TEXT ErrorCode
        TEXT ErrorMessage
    }

    SampleResult {
        INTEGER Id PK
        INTEGER TestRecordId FK
        TEXT SampleBarcode
        INTEGER SamplePosition
        REAL Concentration
        REAL Volume
        TEXT QualityFlag
        TEXT RawDataJson
        TEXT CreatedAt
    }

    ReportSnapshot {
        INTEGER Id PK
        INTEGER TestRecordId FK
        TEXT ReportType
        TEXT GeneratedAt
        TEXT ContentJson
        BLOB PdfBlob
    }

    %% ====== trio240plus_log.db ======
    OperationLog {
        INTEGER Id PK
        TEXT Timestamp
        TEXT Level
        TEXT Category
        TEXT UserName
        TEXT Action
        TEXT Detail
    }

    CommunicationLog {
        INTEGER Id PK
        TEXT Timestamp
        TEXT Direction
        INTEGER FunctionCode
        INTEGER Address
        TEXT DataHex
        INTEGER IsError
    }

    %% ====== Relationships ======
    FlowDefinition ||--o{ FlowStep : "contains"
    TestRecord ||--o{ SampleResult : "has"
    TestRecord ||--o{ ReportSnapshot : "generates"
```

---

## 二、關聯說明

### 2.1 trio240plus_main.db 內部關聯

```mermaid
graph LR
    FD["FlowDefinition<br/>(10 筆)"] -->|"1:N CASCADE"| FS["FlowStep<br/>(2,091 筆)"]
    FM["FlowMapping<br/>(18 筆)"] -.->|"ProductCode 邏輯關聯"| FD
    PM["PnidMapping<br/>(24 筆)"] -.->|"LinkedProductCode 邏輯關聯"| FM
    UA["UserAccount<br/>(3 筆)"] -.->|"OperatorName 邏輯關聯"| TR["TestRecord"]
```

- **FlowDefinition → FlowStep**: 物理外鍵，CASCADE 刪除。刪除流程時自動刪除所有步驟。
- **FlowMapping ↔ FlowDefinition**: 邏輯關聯（透過 FlowName 欄位），無物理外鍵。
- **PnidMapping ↔ FlowMapping**: 邏輯關聯（透過 LinkedProductCode），無物理外鍵。

### 2.2 trio240plus_data.db 內部關聯

```mermaid
graph LR
    TR["TestRecord"] -->|"1:N CASCADE"| SR["SampleResult"]
    TR -->|"1:N CASCADE"| RS["ReportSnapshot"]
```

- **TestRecord → SampleResult**: 物理外鍵，CASCADE 刪除。
- **TestRecord → ReportSnapshot**: 物理外鍵，CASCADE 刪除。

### 2.3 跨庫邏輯關聯（無物理外鍵）

```mermaid
graph TB
    subgraph "config.db"
        CD["CommandDefinition"]
    end
    subgraph "main.db"
        FS2["FlowStep.CommandId"]
        FD2["FlowDefinition.FlowName"]
    end
    subgraph "data.db"
        TR2["TestRecord.FlowName"]
    end
    
    CD -.->|"CommandId 對應"| FS2
    FD2 -.->|"FlowName 對應"| TR2
```

> **設計決策**: 跨庫不建立物理外鍵（SQLite 不支援跨資料庫外鍵），改由應用層確保一致性。

---

## 三、資料流向圖

```mermaid
flowchart TD
    INI["12 個 .ini 檔案"] -->|"Python 解析"| SC["SystemConfig<br/>(2,497 筆)"]
    ENUM["X_Flow_t 列舉"] -->|"C# Seed"| CD2["CommandDefinition<br/>(29 筆)"]
    FLOW[".flow 檔案"] -->|"Python 解析"| FD3["FlowDefinition + FlowStep<br/>(10 + 2,091 筆)"]
    FINI["flowinfo.ini"] -->|"C# Seed"| FM2["FlowMapping + PnidMapping<br/>(18 + 24 筆)"]
    MANUAL["手動建立"] -->|"C# Seed"| UA2["UserAccount<br/>(3 筆)"]
    
    SC --> CONFIG["trio240plus_config.db"]
    CD2 --> CONFIG
    FD3 --> MAIN["trio240plus_main.db"]
    FM2 --> MAIN
    UA2 --> MAIN
    
    RUN["系統運行"] -->|"自動產生"| DATA["trio240plus_data.db"]
    RUN -->|"自動產生"| LOG["trio240plus_log.db"]
```

---

*文件結束*
