// 檢查 + 補齊 config.db 中的 app_close 與 auth 設定
using Microsoft.Data.Sqlite;

var dbPath = @"D:\TRIO2026\Database\trio240plus_config.db";
Console.WriteLine($"目標: {dbPath}\n");

using var conn = new SqliteConnection($"Data Source={dbPath}");
conn.Open();

// 列出現有設定
Console.WriteLine("[現有設定]");
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT Category, [Key], Value, Description FROM SystemConfig ORDER BY Category, [Key]";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"  {reader.GetString(0)}.{reader.GetString(1)} = {reader.GetString(2)} ({reader.GetString(3)})");
}

Console.WriteLine("\n[補齊設定]");
var configs = new[]
{
    ("app_close", "button_enabled",  "1",  "bool", "關閉按鈕是否顯示"),
    ("app_close", "esc_key_enabled", "1",  "bool", "ESC 鍵關閉是否啟用"),
    ("app_close", "alt_f4_enabled",  "1",  "bool", "Alt+F4 關閉是否啟用"),
    ("auth",      "login_required",    "0",  "bool", "是否啟動帳號密碼檢查"),
    ("auth",      "init_wait_seconds", "10", "int",  "Init 畫面等待秒數"),
    ("auth",      "default_role_level","1",  "int",  "免登入時預設角色等級"),
};

foreach (var (category, key, value, dataType, desc) in configs)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        INSERT OR IGNORE INTO SystemConfig (Category, [Key], Value, DataType, Description, ModifiedAt, ModifiedBy)
        VALUES (@cat, @key, @val, @dt, @desc, datetime('now'), 'DbMigrate')
    ";
    cmd.Parameters.AddWithValue("@cat", category);
    cmd.Parameters.AddWithValue("@key", key);
    cmd.Parameters.AddWithValue("@val", value);
    cmd.Parameters.AddWithValue("@dt", dataType);
    cmd.Parameters.AddWithValue("@desc", desc);
    var affected = cmd.ExecuteNonQuery();
    Console.WriteLine($"  {category}.{key}: {(affected > 0 ? "已插入" : "已存在")}");
}

Console.WriteLine("\n[最終狀態]");
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT Category, [Key], Value FROM SystemConfig ORDER BY Category, [Key]";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        Console.WriteLine($"  {reader.GetString(0)}.{reader.GetString(1)} = {reader.GetString(2)}");
}
conn.Close();
Console.WriteLine("\n完成!");
