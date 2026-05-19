# TRIO2026 Seed Data 規格書

> **文件編號**: TRIO2026-DB-004  
> **撰寫**: Office of William  
> **日期**: 2026-04-28  
> **版本**: 1.0  
> **說明**: 初始資料（Seed Data）的來源、格式、植入規則與執行方式  

---

## 一、Seed Data 總覽

| 目標表 | 筆數 | 資料來源 | 植入方式 | 冪等設計 |
|--------|-----:|---------|---------|---------|
| CommandDefinition | 29 | X_Flow_t 列舉 | C# (DbInitializer) | AnyAsync() |
| SystemConfig | 2,497 | 12 個 config/*.ini | Python 腳本 | COUNT(*) 判斷 |
| FlowDefinition | 10 | 10 個 .flow 檔案 | Python 腳本 | COUNT(*) 判斷 |
| FlowStep | 2,091 | 10 個 .flow 步驟 | Python 腳本 | 隨 FlowDefinition |
| FlowMapping | 18 | flowinfo.ini [Flow] | C# (DbInitializer) | AnyAsync() |
| PnidMapping | 24 | flowinfo.ini [PNID] | C# (DbInitializer) | AnyAsync() |
| UserAccount | 3 | 手動定義 | C# (DbInitializer) | AnyAsync() |

---

## 二、植入工具與執行順序

```bash
# 步驟 1: 建表 + C# Seed Data (指令定義/映射/帳號)
dotnet run --project tools/DbInitializer

# 步驟 2: 匯入 12 個 INI → SystemConfig (2,497 筆)
python tools/import_system_config.py

# 步驟 3: 匯入 .flow → FlowDefinition + FlowStep (2,101 筆)
python tools/import_flow_definitions.py

# 步驟 4: 驗證
python tools/verify_schema.py
```

---

## 三、資料來源對應表

### 3.1 SystemConfig — INI 來源對應

| config/*.ini 檔案 | → Category | 筆數 | 內容說明 |
|-------------------|-----------|-----:|---------|
| areaposcfg.ini (27.7KB) | area_position | 1,314 | 16 個區域座標（含 96 孔盤位置） |
| motocfg.ini (5.7KB) | motor | 397 | 16 軸電機配置（速度、行程、脈衝） |
| pipetteinfo.ini (6.7KB) | pipette | 369 | 8 種移液槍頭（校正曲線 seg_0~31） |
| flowinfo.ini (3.3KB) | flow_info | 125 | 產品碼/PNID/模組配置 |
| tubecfg.ini (1.8KB) | tube | 106 | 13 種試管尺寸 |
| opticsinfo.ini (1.2KB) | optics | 83 | 光學校正表/擬合曲線 |
| trioinfo.ini (1.2KB) | trio_info | 56 | 孔位偏移/活塞參數 |
| flowlist.ini (0.7KB) | flow_list | 26 | 流程名稱與步驟數索引 |
| cameracfg.ini (0.3KB) | camera | 10 | 攝像頭掃描區域座標 |
| temperaturecfg.ini (0.3KB) | temperature | 8 | 8 通道加熱器配置 |
| maintenance.ini (0.04KB) | maintenance | 2 | 拆箱維護軸位置 |
| syscfg.ini (0.03KB) | system | 1 | 系統功能模式旗標 |

**Key 格式規則**: `[Section].key`，例如 `[MT12].spd1`、`[area_01].X_start`

### 3.2 CommandDefinition — X_Flow_t 列舉對應

原始碼位置: `commshowwidget.h` 中的 `X_Flow_t` 列舉  
UI 參數邏輯: `xflowstep.cpp` 中的 `slotfunstepchanged()` 函式

C# Seed 檔案: `src/TRIO2026.Data/Seeding/CommandDefinitionSeed.cs`

### 3.3 FlowMapping — flowinfo.ini [Flow] Section

原始格式:
```ini
[Flow]
num=18
P0001="P0001,60,N/A,FFPE-DNA,1-1-1-1-1-1,8100"
P0002="P0002,60,5000,cfDNA,1-3-1-1-1-1,9300"
```

欄位映射:
```
ProductCode = Section key (P0001)
FlowName    = 第 1 欄
ProcessingTimeMin = 第 2 欄
SampleVolumeUl    = 第 3 欄
SampleType        = 第 4 欄
ModuleConfig      = 第 5 欄
EstimatedSeconds  = 第 6 欄
```

### 3.4 .flow 檔案格式

```
##群組名稱                     ← 群組開始
stepOrder,commandId,crc,arg0,arg1,arg2,arg3,arg4,   ← 步驟行
**群組名稱                     ← 群組結束
```

解析規則:
- `##` 行: 壓入群組堆疊，記錄 GroupName 和 GroupDepth
- `**` 行: 彈出群組堆疊
- 數值行: 解析 8 欄 CSV，前 3 欄為 stepOrder/commandId/crc，後 5 欄為 arg0~arg4
- 字串指令（CommandId=22,31,32,33）: arg 可能為字串，存入 StringArg 欄位

---

## 四、安全性注意事項

1. **UserAccount 密碼**: 使用 `$2a$12$PLACEHOLDER_..._DEPLOY` 標記，正式部署前必須用 BCrypt.Net-Next 套件重新生成
2. **不得在 Seed Data 或任何原始碼中出現真實密碼或金鑰**
3. **SystemConfig 中不含敏感資訊**（純硬體參數與校正值）

---

*文件結束*
