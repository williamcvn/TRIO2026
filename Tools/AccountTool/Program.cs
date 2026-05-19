using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

// ══════════════════════════════════════════════════════════════
// TRIO2026 帳號密碼測試工具（開發階段專用，不部署）
// 
// 讀取 main.db 的 User 表
// 功能：
//   [1] 列出所有帳號
//   [2] 驗證密碼（輸入帳號+密碼 → 驗證是否正確）
//   [3] 重設密碼（輸入帳號+新密碼 → 更新 DB）
// 
// 製作者: Office of William
// ══════════════════════════════════════════════════════════════

var dbPath = @"D:\TRIO2026\Database\main.db";

if (!System.IO.File.Exists(dbPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n  ❌ 找不到資料庫: {dbPath}");
    Console.ResetColor();
    return;
}

while (true)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("  ╔══════════════════════════════════════╗");
    Console.WriteLine("  ║   🔑 TRIO2026 帳號密碼測試工具       ║");
    Console.WriteLine("  ╠══════════════════════════════════════╣");
    Console.WriteLine("  ║   [1]  列出所有帳號                  ║");
    Console.WriteLine("  ║   [2]  驗證密碼                      ║");
    Console.WriteLine("  ║   [3]  重設密碼                      ║");
    Console.WriteLine("  ║   [0]  離開                          ║");
    Console.WriteLine("  ╚══════════════════════════════════════╝");
    Console.ResetColor();
    Console.Write("\n  請選擇 [0-3]: ");
    var choice = Console.ReadLine()?.Trim() ?? "";

    switch (choice)
    {
        case "1": ListAccounts(); break;
        case "2": VerifyPassword(); break;
        case "3": ResetPassword(); break;
        case "0": return;
        default: Console.WriteLine("  無效選項"); break;
    }
}

// ── 列出所有帳號 ──
void ListAccounts()
{
    using var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        SELECT Id, Username, DisplayName, RoleLevel, IsActive, 
               ForcePasswordChange, FailedLoginCount, LockedUntil,
               LastLoginAt, EmployeeId, Department
        FROM User
        ORDER BY Id";

    var reader = cmd.ExecuteReader();

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  ┌────┬──────────────┬──────────────────┬───────┬────────┬──────────┬─────────────────────┐");
    Console.WriteLine("  │ ID │ 帳號         │ 顯示名稱         │ Level │ 啟用   │ 需改密碼 │ 上次登入            │");
    Console.WriteLine("  ├────┼──────────────┼──────────────────┼───────┼────────┼──────────┼─────────────────────┤");
    Console.ResetColor();

    var roleName = new Dictionary<int, string> { [1] = "Operator", [2] = "Service", [3] = "Admin" };

    while (reader.Read())
    {
        var id = reader.GetInt32(0);
        var username = reader.GetString(1);
        var display = reader.IsDBNull(2) ? "" : reader.GetString(2);
        var level = reader.GetInt32(3);
        var active = reader.GetInt32(4) == 1;
        var forceChange = !reader.IsDBNull(5) && reader.GetInt32(5) == 1;
        var failedCount = reader.GetInt32(6);
        var locked = !reader.IsDBNull(7) ? reader.GetString(7) : "";
        var lastLogin = reader.IsDBNull(8) ? "-" : reader.GetString(8);
        if (lastLogin.Length > 19) lastLogin = lastLogin[..19];

        var role = roleName.GetValueOrDefault(level, $"Level {level}");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  │ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{id,-3}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("│ ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{Truncate(username, 12),-12}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("│ ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"{Truncate(display, 16),-16}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("│ ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{level} {Truncate(role, 3),-3}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("│ ");
        Console.ForegroundColor = active ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write($"{(active ? "✅" : "❌"),-6}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("│ ");
        Console.ForegroundColor = forceChange ? ConsoleColor.Yellow : ConsoleColor.DarkGray;
        Console.Write($"{(forceChange ? "⚠️ 是" : "  否"),-8}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("│ ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"{lastLogin,-19}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("│");

        // 鎖定/失敗次數提示
        if (!string.IsNullOrEmpty(locked) || failedCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │    │              │                  │       │        │          │ ");
            Console.ForegroundColor = ConsoleColor.Red;
            if (!string.IsNullOrEmpty(locked) && locked.Length >= 19)
                Console.Write($"🔒 鎖定至 {locked[..19]}");
            else if (failedCount > 0)
                Console.Write($"⚠️ 失敗 {failedCount} 次");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("│");
        }
    }

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  └────┴──────────────┴──────────────────┴───────┴────────┴──────────┴─────────────────────┘");
    Console.ResetColor();
}

// ── 驗證密碼 ──
void VerifyPassword()
{
    Console.Write("\n  輸入帳號: ");
    var username = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrEmpty(username)) return;

    Console.Write("  輸入密碼: ");
    var password = ReadPasswordMasked();

    using var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT PasswordHash, RoleLevel, DisplayName FROM [User] WHERE Username = @u";
    cmd.Parameters.AddWithValue("@u", username);

    var reader = cmd.ExecuteReader();
    if (!reader.Read())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n  ❌ 找不到帳號: {username}");
        Console.ResetColor();
        return;
    }

    var hash = reader.GetString(0);
    var level = reader.GetInt32(1);
    var display = reader.IsDBNull(2) ? "" : reader.GetString(2);

    try
    {
        var valid = BCrypt.Net.BCrypt.Verify(password, hash);
        if (valid)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✅ 密碼正確！");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"     帳號: {username} ({display})");
            Console.WriteLine($"     角色: RoleLevel={level}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  ❌ 密碼錯誤");
        }
    }
    catch
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n  ❌ 密碼雜湊格式異常（可能未正確初始化）");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"     Hash: {hash[..Math.Min(30, hash.Length)]}...");
    }
    Console.ResetColor();
}

// ── 重設密碼 ──
void ResetPassword()
{
    Console.Write("\n  輸入帳號: ");
    var username = Console.ReadLine()?.Trim() ?? "";
    if (string.IsNullOrEmpty(username)) return;

    Console.Write("  輸入新密碼: ");
    var newPassword = ReadPasswordMasked();
    if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  ❌ 密碼太短（至少 4 字元）");
        Console.ResetColor();
        return;
    }

    Console.Write("  確認新密碼: ");
    var confirm = ReadPasswordMasked();
    if (newPassword != confirm)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  ❌ 兩次輸入不一致");
        Console.ResetColor();
        return;
    }

    var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);

    using var conn = new SqliteConnection($"Data Source={dbPath}");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"UPDATE [User] 
        SET PasswordHash = @hash, ForcePasswordChange = 0, FailedLoginCount = 0, LockedUntil = NULL,
            PasswordChangedAt = @now, UpdatedAt = @now, UpdatedBy = 'AccountTool'
        WHERE Username = @u";
    cmd.Parameters.AddWithValue("@hash", newHash);
    cmd.Parameters.AddWithValue("@u", username);
    cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("O"));

    var affected = cmd.ExecuteNonQuery();
    if (affected > 0)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  ✅ 密碼已重設！帳號: {username}");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"     (鎖定已清除, ForcePasswordChange 已關閉)");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n  ❌ 找不到帳號: {username}");
    }
    Console.ResetColor();
}

// ── 密碼遮罩輸入 ──
string ReadPasswordMasked()
{
    var pwd = new List<char>();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) break;
        if (key.Key == ConsoleKey.Backspace)
        {
            if (pwd.Count > 0)
            {
                pwd.RemoveAt(pwd.Count - 1);
                Console.Write("\b \b");
            }
        }
        else
        {
            pwd.Add(key.KeyChar);
            Console.Write("*");
        }
    }
    Console.WriteLine();
    return new string(pwd.ToArray());
}

static string Truncate(string s, int max)
{
    if (string.IsNullOrEmpty(s)) return "";
    return s.Length <= max ? s : s[..(max - 2)] + "..";
}
