using TRIO2026.Core.Entities;
using TRIO2026.Core.Enums;
using TRIO2026.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace TRIO2026.App.Services;

/// <summary>
/// 認證服務 — 負責登入驗證、密碼雜湊、鎖定機制
/// 
/// 資料來源：main.db 的 User 表
/// 
/// 製作者: Office of William
/// </summary>
public class AuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    private readonly AppMainDbContext _db;

    public AuthService(AppMainDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 驗證使用者帳號密碼
    /// </summary>
    public async Task<(AuthResult Result, User? User)> LoginAsync(string username, string password)
    {
        // 清除 Change Tracker 快取，確保從 DB 讀取最新資料
        // （WPF 中 DbContext 為長生命週期，外部工具修改 DB 後需要此步驟）
        _db.ChangeTracker.Clear();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return (AuthResult.UserNotFound, null);
        }

        if (user.IsActive == 0)
        {
            return (AuthResult.AccountDisabled, null);
        }

        // 檢查鎖定狀態
        if (!string.IsNullOrEmpty(user.LockedUntil))
        {
            if (DateTime.TryParse(user.LockedUntil, out var lockedUntil))
            {
                if (DateTime.UtcNow < lockedUntil)
                {
                    return (AuthResult.AccountLocked, null);
                }
                // 鎖定已過期，清除
                user.LockedUntil = null;
                user.FailedLoginCount = 0;
            }
        }

        // 驗證密碼
        bool isValid;
        try
        {
            isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch
        {
            // 如果 PasswordHash 格式不正確（如 PLACEHOLDER），直接比對字串
            isValid = false;
        }

        if (!isValid)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedAttempts)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes).ToString("O");
                await _db.SaveChangesAsync();
                return (AuthResult.AccountLocked, null);
            }
            await _db.SaveChangesAsync();
            return (AuthResult.WrongPassword, null);
        }

        // 登入成功
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow.ToString("O");
        await _db.SaveChangesAsync();

        return (AuthResult.Success, user);
    }

    /// <summary>
    /// 雜湊密碼（用於建立帳號或變更密碼）
    /// </summary>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// 取得所有使用者（用於帳號下拉選單）
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _db.Users
            .Where(u => u.IsActive == 1)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    /// <summary>
    /// 更新使用者的語系偏好
    /// </summary>
    public async Task UpdateLanguagePreferenceAsync(int userId, string langCode)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.LanguagePreference = langCode;
            await _db.SaveChangesAsync();
        }
    }
}
