// 資料庫遷移工具：從 E:\temp\Database 的舊 DB 匯入資料到 D:\TRIO2026\Database 的新 DB
// 處理 schema 差異（新 DB 有 RoleDefinition 表）
using Microsoft.Data.Sqlite;

var oldDir = @"E:\temp\Database";
var newDir = @"D:\TRIO2026\Database";

Console.WriteLine("=== TRIO2026 DB 資料遷移工具 ===\n");

// 1. 先停止 App（確保 DB 不被鎖定）
Console.WriteLine("請確認 TRIO2026.App 與 DevLauncher 已關閉\n");

// 2. 對每個 DB 進行遷移
var dbFiles = new[] { "trio240plus_config.db", "trio240plus_log.db", "trio240plus_data.db" };

foreach (var dbFile in dbFiles)
{
    var oldPath = Path.Combine(oldDir, dbFile);
    var newPath = Path.Combine(newDir, dbFile);

    if (!File.Exists(oldPath))
    {
        Console.WriteLine($"[跳過] {dbFile} — 舊檔案不存在");
        continue;
    }

    // 直接複製（這些 DB 沒有 schema 變更）
    File.Copy(oldPath, newPath, overwrite: true);
    Console.WriteLine($"[複製] {dbFile} — 從 E:\\temp\\Database 複製完成");
}

// 3. main.db 需要特殊處理（新增了 RoleDefinition 表）
Console.WriteLine("\n--- trio240plus_main.db 遷移 ---");
var oldMainPath = Path.Combine(oldDir, "trio240plus_main.db");
var newMainPath = Path.Combine(newDir, "trio240plus_main.db");

if (File.Exists(oldMainPath))
{
    // 讀取舊 DB 的所有表和資料
    using var oldConn = new SqliteConnection($"Data Source={oldMainPath};Mode=ReadOnly");
    oldConn.Open();

    // 取得舊 DB 的表清單
    var tables = new List<string>();
    using (var cmd = oldConn.CreateCommand())
    {
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name";
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) tables.Add(reader.GetString(0));
    }
    Console.WriteLine($"舊 DB 表: {string.Join(", ", tables)}");

    // 列出每個表的資料筆數
    foreach (var table in tables)
    {
        using var cmd = oldConn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM [{table}]";
        var count = Convert.ToInt64(cmd.ExecuteScalar());
        Console.WriteLine($"  {table}: {count} 筆");
    }

    // 方案：刪除新 DB，複製舊 DB，然後添加 RoleDefinition 表
    oldConn.Close();

    File.Copy(oldMainPath, newMainPath, overwrite: true);
    Console.WriteLine($"[複製] trio240plus_main.db 完成");

    // 在複製的 DB 上添加 RoleDefinition 表
    using var newConn = new SqliteConnection($"Data Source={newMainPath}");
    newConn.Open();

    // 檢查 RoleDefinition 是否已存在
    using (var checkCmd = newConn.CreateCommand())
    {
        checkCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='RoleDefinition'";
        var exists = Convert.ToInt64(checkCmd.ExecuteScalar()) > 0;

        if (!exists)
        {
            using var createCmd = newConn.CreateCommand();
            createCmd.CommandText = @"
                CREATE TABLE RoleDefinition (
                    Level INTEGER PRIMARY KEY,
                    Code TEXT NOT NULL,
                    DisplayName TEXT NOT NULL,
                    Description TEXT,
                    CreatedAt TEXT NOT NULL
                );
                CREATE UNIQUE INDEX IX_RoleDefinition_Code ON RoleDefinition(Code);

                INSERT INTO RoleDefinition (Level, Code, DisplayName, Description, CreatedAt) VALUES
                    (1, 'Operator', '操作員', '基本操作權限 — 執行流程、查看報表', datetime('now')),
                    (2, 'Service', 'Service 工程師', '系統設定 + 進階維護 — 校正、參數調整、流程編輯', datetime('now')),
                    (3, 'Admin', '管理員', '全部權限 — 帳號管理、系統組態、資料匯出', datetime('now'));
            ";
            createCmd.ExecuteNonQuery();
            Console.WriteLine("[新增] RoleDefinition 表已建立 + 3 筆種子資料");
        }
        else
        {
            Console.WriteLine("[略過] RoleDefinition 表已存在");
        }
    }

    newConn.Close();
}

Console.WriteLine("\n=== 遷移完成 ===");
