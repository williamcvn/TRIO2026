using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TRIO2026.Core.Entities;
using TRIO2026.Data.Contexts;

namespace TRIO2026.App.Services;

/// <summary>
/// UV 照射配置服務 — 從 system_config.db 讀取 UvTimerOption
/// 
/// 排序規則：
///   - 所有結果一律依 DurationSeconds 由小到大排序
///   - 不依賴 DB 的 SortOrder，確保 UI 呈現一致性
/// 
/// 預設選項規則：
///   - 取 IsDefault=1 且 IsEnabled=1 的項目
///   - 若有多筆 IsDefault=1，取 DurationSeconds 最小者
///   - 若無 IsDefault=1，取 DurationSeconds 最小的啟用項目
/// 
/// 使用 IServiceProvider 建立 Scope 存取 DbContext，
/// 避免 DbContext 生命週期問題。
/// 
/// 製作者: Office of William
/// </summary>
public class UvConfigService
{
    private readonly IServiceProvider _serviceProvider;

    public UvConfigService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 取得所有已啟用的 UV 時間選項（依 DurationSeconds 由小到大排序）
    /// </summary>
    public async Task<List<UvTimerOption>> GetEnabledOptionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SystemConfigDbContext>();

        return await context.UvTimerOptions
            .Where(o => o.IsEnabled == 1)
            .OrderBy(o => o.DurationSeconds)
            .ToListAsync();
    }

    /// <summary>
    /// 取得預設選項
    /// 規則：IsDefault=1 且 IsEnabled=1，多筆時取 DurationSeconds 最小者
    /// 若無 IsDefault，取 DurationSeconds 最小的啟用項目
    /// </summary>
    public async Task<UvTimerOption?> GetDefaultOptionAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SystemConfigDbContext>();

        // 優先：IsDefault=1 且 IsEnabled=1，多筆取秒數最小
        var defaultOption = await context.UvTimerOptions
            .Where(o => o.IsEnabled == 1 && o.IsDefault == 1)
            .OrderBy(o => o.DurationSeconds)
            .FirstOrDefaultAsync();

        // 備用：無預設時取秒數最小的啟用項目
        return defaultOption
            ?? await context.UvTimerOptions
                .Where(o => o.IsEnabled == 1)
                .OrderBy(o => o.DurationSeconds)
                .FirstOrDefaultAsync();
    }
}
