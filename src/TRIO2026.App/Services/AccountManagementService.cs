using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TRIO2026.Core.Entities;
using TRIO2026.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace TRIO2026.App.Services;

/// <summary>
/// 帳號管理服務 — Admin 專用
/// 所有方法須確認呼叫者為 Admin (RoleLevel=3)
/// 
/// 功能：
///   - 查詢帳號列表（排除 local_operator 和已刪除帳號）
///   - 新增帳號（Operator / Admin，產生臨時密碼）
///   - 假刪除帳號（Soft Delete）
///   - 停用 / 啟用帳號
///   - 鎖定 / 解鎖帳號
///   - 重設帳號密碼（產生臨時密碼）
/// 
/// 製作者: Office of William
/// </summary>
public partial class AccountManagementService
{
    private readonly AppMainDbContext _db;
    private readonly SystemSettingService _systemSettings;

    public AccountManagementService(AppMainDbContext db, SystemSettingService systemSettings)
    {
        _db = db;
        _systemSettings = systemSettings;
    }

    // ═══════════════════════════════════════
    // 查詢
    // ═══════════════════════════════════════

    /// <summary>
    /// 取得所有非 local_operator、IsDeleted=0 的帳號清單（含停用帳號）
    /// 排序：Admin → Service → Operator，同角色按 DisplayName 排序，停用帳號排最後
    /// </summary>
    public async Task<List<User>> GetAllManagedUsersAsync()
    {
        _db.ChangeTracker.Clear();
        return await _db.Users
            .Where(u => u.IsDeleted == 0 && u.Username != "local_operator")
            .OrderByDescending(u => u.RoleLevel)
            .ThenBy(u => u.IsActive == 0 ? 1 : 0) // 停用排最後
            .ThenBy(u => u.DisplayName ?? u.Username)
            .ToListAsync();
    }

    // ═══════════════════════════════════════
    // 建立
    // ═══════════════════════════════════════

    /// <summary>
    /// 新增帳號，回傳臨時密碼明文（只呼叫一次）
    /// </summary>
    public async Task<(bool Success, string? Error, string? TempPassword)>
        CreateUserAsync(string username, string? displayName, int roleLevel, string createdBy)
    {
        _db.ChangeTracker.Clear();

        // 驗證 username 格式（英數字 + 底線，3~20 字元）
        if (!UsernameRegex().IsMatch(username))
            return (false, "INVALID_USERNAME", null);

        // 驗證角色：僅限 Operator(1) / Admin(3)
        if (roleLevel != 1 && roleLevel != 3)
            return (false, "INVALID_ROLE", null);

        // 檢查 Username 唯一性（含已刪除帳號）
        var exists = await _db.Users.AnyAsync(u => u.Username == username);
        if (exists)
            return (false, "USERNAME_EXISTS", null);

        // 產生 12 碼隨機臨時密碼
        var tempPassword = GenerateRandomPassword(12);
        var now = DateTime.UtcNow.ToString("O");

        var user = new User
        {
            Username = username,
            DisplayName = displayName ?? username,
            PasswordHash = AuthService.HashPassword(tempPassword),
            RoleLevel = roleLevel,
            IsActive = 1,
            IsDeleted = 0,
            ForcePasswordChange = 1, // 強制首次登入變更密碼
            CreatedAt = now,
            CreatedBy = createdBy,
            UpdatedAt = now,
            UpdatedBy = createdBy,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (true, null, tempPassword);
    }

    // ═══════════════════════════════════════
    // 假刪除
    // ═══════════════════════════════════════

    /// <summary>
    /// 假刪除帳號（含安全守衛）
    /// </summary>
    public async Task<(bool Success, string? Error)>
        DeleteUserAsync(int userId, string operatorUsername)
    {
        _db.ChangeTracker.Clear();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted == 0);
        if (user == null)
            return (false, "User not found.");

        // 安全守衛：不可刪除 local_operator
        if (user.Username == "local_operator")
            return (false, "ERROR_SELF");

        // 安全守衛：不可刪除自己
        if (user.Username == operatorUsername)
            return (false, "ERROR_SELF");

        // 安全守衛：Service 帳號不可從 UI 刪除
        if (user.RoleLevel == 2)
            return (false, "ERROR_SERVICE_DELETE");

        // 安全守衛：不可刪除唯一啟用且未刪除的 Admin
        if (user.RoleLevel == 3)
        {
            var activeAdmins = await CountActiveAdminsAsync();
            if (activeAdmins <= 1)
                return (false, "ERROR_LAST_ADMIN");
        }

        // 執行假刪除
        user.IsDeleted = 1;
        user.IsActive = 0;
        user.DeletedAt = DateTime.UtcNow.ToString("O");
        user.DeletedBy = operatorUsername;
        user.UpdatedAt = DateTime.UtcNow.ToString("O");
        user.UpdatedBy = operatorUsername;
        await _db.SaveChangesAsync();

        return (true, null);
    }

    // ═══════════════════════════════════════
    // 狀態變更
    // ═══════════════════════════════════════

    /// <summary>停用 / 啟用帳號</summary>
    public async Task<(bool Success, string? Error)>
        SetActiveAsync(int userId, bool active, string operatorUsername)
    {
        _db.ChangeTracker.Clear();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted == 0);
        if (user == null)
            return (false, "User not found.");

        // 安全守衛：不可停用自己
        if (!active && user.Username == operatorUsername)
            return (false, "ERROR_SELF");

        // 安全守衛：不可停用唯一啟用的 Admin
        if (!active && user.RoleLevel == 3)
        {
            var activeAdmins = await CountActiveAdminsAsync();
            if (activeAdmins <= 1)
                return (false, "ERROR_LAST_ADMIN");
        }

        user.IsActive = active ? 1 : 0;
        user.UpdatedAt = DateTime.UtcNow.ToString("O");
        user.UpdatedBy = operatorUsername;
        await _db.SaveChangesAsync();

        return (true, null);
    }

    /// <summary>鎖定帳號（LockedUntil = 9999-12-31，永久直到手動解鎖）</summary>
    public async Task<(bool Success, string? Error)>
        LockUserAsync(int userId, string operatorUsername)
    {
        _db.ChangeTracker.Clear();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted == 0);
        if (user == null)
            return (false, "User not found.");

        // 安全守衛：不可鎖定自己
        if (user.Username == operatorUsername)
            return (false, "ERROR_SELF");

        user.LockedUntil = "9999-12-31T23:59:59.0000000+00:00";
        user.UpdatedAt = DateTime.UtcNow.ToString("O");
        user.UpdatedBy = operatorUsername;
        await _db.SaveChangesAsync();

        return (true, null);
    }

    /// <summary>解鎖帳號（LockedUntil = null, FailedLoginCount = 0）</summary>
    public async Task<(bool Success, string? Error)>
        UnlockUserAsync(int userId, string operatorUsername)
    {
        _db.ChangeTracker.Clear();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted == 0);
        if (user == null)
            return (false, "User not found.");

        user.LockedUntil = null;
        user.FailedLoginCount = 0;
        user.UpdatedAt = DateTime.UtcNow.ToString("O");
        user.UpdatedBy = operatorUsername;
        await _db.SaveChangesAsync();

        return (true, null);
    }

    // ═══════════════════════════════════════
    // 密碼管理
    // ═══════════════════════════════════════

    /// <summary>
    /// Admin 重設指定帳號密碼，回傳臨時密碼明文（只呼叫一次）
    /// 同時設定 ForcePasswordChange=1，清除 LockedUntil / FailedLoginCount
    /// </summary>
    public async Task<(bool Success, string? Error, string? TempPassword)>
        ResetPasswordAsync(int userId, string operatorUsername)
    {
        _db.ChangeTracker.Clear();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted == 0);
        if (user == null)
            return (false, "User not found.", null);

        // 產生 12 碼隨機臨時密碼
        var tempPassword = GenerateRandomPassword(12);

        user.PasswordHash = AuthService.HashPassword(tempPassword);
        user.ForcePasswordChange = 1;
        user.LockedUntil = null;
        user.FailedLoginCount = 0;
        user.PasswordChangedAt = null; // 等使用者自行更新
        user.UpdatedAt = DateTime.UtcNow.ToString("O");
        user.UpdatedBy = operatorUsername;
        await _db.SaveChangesAsync();

        return (true, null, tempPassword);
    }

    // ═══════════════════════════════════════
    // 安全守衛輔助
    // ═══════════════════════════════════════

    /// <summary>確認系統中啟用且未刪除的 Admin 帳號數量</summary>
    public async Task<int> CountActiveAdminsAsync()
    {
        return await _db.Users
            .CountAsync(u => u.RoleLevel == 3 && u.IsActive == 1 && u.IsDeleted == 0);
    }

    /// <summary>預留：更新使用者基本資料（未來版本實作）</summary>
    public Task<(bool Success, string? Error)> UpdateUserProfileAsync(
        int userId, string? displayName, string? department, string? email, string? notes)
    {
        // 暫不實作，預留擴充點
        throw new NotImplementedException("UpdateUserProfileAsync is reserved for future versions.");
    }

    // ═══════════════════════════════════════
    // 輔助方法
    // ═══════════════════════════════════════

    /// <summary>產生指定長度的隨機密碼（大小寫英數混合）</summary>
    private static string GenerateRandomPassword(int length)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";  // 排除 I, O 避免混淆
        const string lower = "abcdefghjkmnpqrstuvwxyz";    // 排除 l, o 避免混淆
        const string digits = "23456789";                    // 排除 0, 1 避免混淆
        const string all = upper + lower + digits;

        var result = new char[length];
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        // 確保至少包含一個大寫、小寫、數字
        result[0] = upper[bytes[0] % upper.Length];
        result[1] = lower[bytes[1] % lower.Length];
        result[2] = digits[bytes[2] % digits.Length];

        for (int i = 3; i < length; i++)
            result[i] = all[bytes[i] % all.Length];

        // Fisher-Yates shuffle
        for (int i = length - 1; i > 0; i--)
        {
            var j = bytes[i] % (i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return new string(result);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_]{3,20}$")]
    private static partial Regex UsernameRegex();
}
