using System.ComponentModel;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TRIO2026.Data.Contexts;

namespace TRIO2026.App.Services;

/// <summary>
/// DB 驅動的多語系服務
/// 
/// 從 system_config.db → LocalizedString 表讀取翻譯字串，
/// 支援即時語系切換（透過 INotifyPropertyChanged 通知 XAML 重新渲染）。
/// 
/// XAML 綁定範例：
///   Text="{Binding [UV.Title], Source={x:Static svc:LocalizationService.Instance}}"
/// 
/// 語系切換範例：
///   LocalizationService.Instance.SwitchLanguageAsync("zh-TW");
/// 
/// 支援語系: en, zh-TW, zh-CN, ja
/// 
/// 注意：此服務為 Singleton，透過 IServiceProvider 建立 Scope 來存取 DbContext，
///       避免 Singleton 依賴 Scoped 服務的 DI 衝突。
/// </summary>
public class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, string> _strings = new();
    private string _currentLanguage = "en";

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>靜態單例（供 XAML x:Static 綁定使用）</summary>
    public static LocalizationService Instance
    {
        get => _instance ?? throw new InvalidOperationException(
            "LocalizationService 尚未初始化，請先呼叫 InitializeAsync()");
    }

    /// <summary>當前語系代碼</summary>
    public string CurrentLanguage => _currentLanguage;

    /// <summary>
    /// 索引器 — XAML Binding 使用
    /// Key 格式: "Module.ResourceKey"（例: "UV.Title", "Common.OK"）
    /// </summary>
    public string this[string key]
    {
        get
        {
            if (_strings.TryGetValue(key, out var value))
                return value;

            Debug.WriteLine($"[i18n] 找不到翻譯: {key} ({_currentLanguage})");
            return $"[{key}]"; // 顯示 key 名稱作為 fallback
        }
    }

    public LocalizationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _instance = this;
    }

    /// <summary>
    /// 初始化 — 載入指定語系的所有字串
    /// </summary>
    public async Task InitializeAsync(string languageCode = "en")
    {
        _currentLanguage = languageCode;
        await LoadStringsAsync();
    }

    /// <summary>
    /// 切換語系 — 重新載入字串並通知所有 XAML 綁定更新
    /// </summary>
    public async Task SwitchLanguageAsync(string languageCode)
    {
        if (_currentLanguage == languageCode) return;

        _currentLanguage = languageCode;
        await LoadStringsAsync();

        // 通知所有綁定更新（Binding 的 Indexer 使用 Item[] 通知）
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }

    /// <summary>
    /// 從 DB 載入指定語系的所有字串到記憶體字典
    /// 使用 IServiceProvider 建立 Scope 來取得 Scoped 的 DbContext
    /// </summary>
    private async Task LoadStringsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SystemConfigDbContext>();

        var records = await context.LocalizedStrings
            .Where(s => s.LanguageCode == _currentLanguage)
            .ToListAsync();

        _strings = records.ToDictionary(
            r => $"{r.Module}.{r.ResourceKey}",
            r => r.Value);

        Debug.WriteLine($"[i18n] 已載入 {_strings.Count} 筆翻譯 ({_currentLanguage})");
    }
}
