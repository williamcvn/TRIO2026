using System.Text.RegularExpressions;

namespace TRIO2026.App.Services;

/// <summary>
/// 密碼原則驗證服務 — 依角色分級驗證密碼複雜度
/// 
/// 所有規則由 SystemSettingService 即時從 DB 讀取，
/// 修改 DB 不需重啟即可生效。
/// 
/// 角色分級：
///   - RoleLevel=1 (Operator) → Operator 規則（允許純數字 PIN）
///   - RoleLevel≥2 (Service/Admin) → Admin 規則（要求複雜密碼）
/// 
/// 密碼原則僅在「設定/變更密碼」時驗證，不在「登入」時驗證。
/// 
/// 製作者: Office of William
/// </summary>
public partial class PasswordPolicyService
{
    private readonly SystemSettingService _settings;

    public PasswordPolicyService(SystemSettingService settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// 驗證密碼是否符合指定角色的原則
    /// </summary>
    /// <param name="password">待驗證密碼</param>
    /// <param name="roleLevel">角色等級：1=Operator, 2=Service, 3=Admin</param>
    /// <returns>null = 通過；string = 第一個不符合的原則錯誤訊息</returns>
    public string? Validate(string password, int roleLevel)
    {
        // 總開關：disabled 時不檢查
        if (!_settings.PasswordPolicyEnabled)
            return null;

        // 空密碼
        if (string.IsNullOrWhiteSpace(password))
            return GetLocalized("PasswordUI.PasswordEmpty");

        if (roleLevel == 1)
            return ValidateOperator(password);
        else
            return ValidateAdmin(password);
    }

    /// <summary>
    /// 取得指定角色的所有驗證規則與目前是否通過
    /// </summary>
    /// <param name="password">當前輸入的密碼（可為 null/空）</param>
    /// <param name="roleLevel">角色等級</param>
    /// <returns>規則列表（描述 + 是否通過）</returns>
    public List<PolicyRule> GetPolicyRules(string? password, int roleLevel)
    {
        var rules = new List<PolicyRule>();
        password ??= "";

        if (roleLevel == 1)
        {
            rules.Add(new PolicyRule(
                string.Format(GetLocalized("PasswordUI.RuleMinLength"), _settings.OperatorMinLength),
                password.Length >= _settings.OperatorMinLength));

            rules.Add(new PolicyRule(
                string.Format(GetLocalized("PasswordUI.RuleMaxLength"), _settings.OperatorMaxLength),
                password.Length == 0 || password.Length <= _settings.OperatorMaxLength));

            // 動態數字鍵盤模式：忽略複雜度規則
            if (!_settings.NumericKeypadOnly)
            {
                if (_settings.OperatorRequireMixed)
                {
                    rules.Add(new PolicyRule(
                        GetLocalized("PasswordUI.RuleRequireMixed"),
                        HasLetter(password) && HasDigit(password)));
                }

                if (_settings.OperatorRequireSpecial)
                {
                    rules.Add(new PolicyRule(
                        GetLocalized("PasswordUI.RuleRequireSpecial"),
                        HasSpecial(password)));
                }
            }
        }
        else
        {
            rules.Add(new PolicyRule(
                string.Format(GetLocalized("PasswordUI.RuleMinLength"), _settings.AdminMinLength),
                password.Length >= _settings.AdminMinLength));

            rules.Add(new PolicyRule(
                string.Format(GetLocalized("PasswordUI.RuleMaxLength"), _settings.AdminMaxLength),
                password.Length == 0 || password.Length <= _settings.AdminMaxLength));

            // 動態數字鍵盤模式：忽略複雜度規則
            if (!_settings.NumericKeypadOnly)
            {
                if (_settings.AdminRequireUpper)
                {
                    rules.Add(new PolicyRule(
                        GetLocalized("PasswordUI.RuleRequireUpper"),
                        HasUpper(password)));
                }

                if (_settings.AdminRequireLower)
                {
                    rules.Add(new PolicyRule(
                        GetLocalized("PasswordUI.RuleRequireLower"),
                        HasLower(password)));
                }

                if (_settings.AdminRequireDigit)
                {
                    rules.Add(new PolicyRule(
                        GetLocalized("PasswordUI.RuleRequireDigit"),
                        HasDigit(password)));
                }

                if (_settings.AdminRequireSpecial)
                {
                    rules.Add(new PolicyRule(
                        GetLocalized("PasswordUI.RuleRequireSpecial"),
                        HasSpecial(password)));
                }
            }
        }

        return rules;
    }

    /// <summary>
    /// 取得密碼原則的文字提示（用於 UI 顯示）
    /// </summary>
    public string GetPolicyHint(int roleLevel)
    {
        if (roleLevel == 1)
        {
            var hint = string.Format(GetLocalized("PasswordUI.RuleMinLength"), _settings.OperatorMinLength)
                     + " ~ " + string.Format(GetLocalized("PasswordUI.RuleMaxLength"), _settings.OperatorMaxLength);
            if (_settings.OperatorRequireMixed) hint += ", " + GetLocalized("PasswordUI.RuleRequireMixed");
            if (_settings.OperatorRequireSpecial) hint += ", " + GetLocalized("PasswordUI.RuleRequireSpecial");
            return hint;
        }
        else
        {
            var hint = string.Format(GetLocalized("PasswordUI.RuleMinLength"), _settings.AdminMinLength)
                     + " ~ " + string.Format(GetLocalized("PasswordUI.RuleMaxLength"), _settings.AdminMaxLength);
            if (_settings.AdminRequireUpper) hint += ", " + GetLocalized("PasswordUI.RuleRequireUpper");
            if (_settings.AdminRequireLower) hint += ", " + GetLocalized("PasswordUI.RuleRequireLower");
            if (_settings.AdminRequireDigit) hint += ", " + GetLocalized("PasswordUI.RuleRequireDigit");
            if (_settings.AdminRequireSpecial) hint += ", " + GetLocalized("PasswordUI.RuleRequireSpecial");
            return hint;
        }
    }

    // ═══════════════════════════════════════
    // Private — 角色驗證邏輯
    // ═══════════════════════════════════════

    private string? ValidateOperator(string password)
    {
        if (password.Length < _settings.OperatorMinLength)
            return string.Format(GetLocalized("PasswordUI.RuleMinLength"), _settings.OperatorMinLength);

        if (password.Length > _settings.OperatorMaxLength)
            return string.Format(GetLocalized("PasswordUI.RuleMaxLength"), _settings.OperatorMaxLength);

        // BCrypt 72B 硬限
        if (password.Length > 72)
            return GetLocalized("PasswordUI.BCryptLimit");

        // 動態數字鍵盤模式：忽略複雜度規則
        if (!_settings.NumericKeypadOnly)
        {
            if (_settings.OperatorRequireMixed && !(HasLetter(password) && HasDigit(password)))
                return GetLocalized("PasswordUI.RuleRequireMixed");

            if (_settings.OperatorRequireSpecial && !HasSpecial(password))
                return GetLocalized("PasswordUI.RuleRequireSpecial");
        }

        return null;
    }

    private string? ValidateAdmin(string password)
    {
        if (password.Length < _settings.AdminMinLength)
            return string.Format(GetLocalized("PasswordUI.RuleMinLength"), _settings.AdminMinLength);

        if (password.Length > _settings.AdminMaxLength)
            return string.Format(GetLocalized("PasswordUI.RuleMaxLength"), _settings.AdminMaxLength);

        // BCrypt 72B 硬限
        if (password.Length > 72)
            return GetLocalized("PasswordUI.BCryptLimit");

        // 動態數字鍵盤模式：忽略複雜度規則
        if (!_settings.NumericKeypadOnly)
        {
            if (_settings.AdminRequireUpper && !HasUpper(password))
                return GetLocalized("PasswordUI.RuleRequireUpper");

            if (_settings.AdminRequireLower && !HasLower(password))
                return GetLocalized("PasswordUI.RuleRequireLower");

            if (_settings.AdminRequireDigit && !HasDigit(password))
                return GetLocalized("PasswordUI.RuleRequireDigit");

            if (_settings.AdminRequireSpecial && !HasSpecial(password))
                return GetLocalized("PasswordUI.RuleRequireSpecial");
        }

        return null;
    }

    // ═══════════════════════════════════════
    // Private — 字元檢測
    // ═══════════════════════════════════════

    private static bool HasLetter(string s) => s.Any(char.IsLetter);
    private static bool HasDigit(string s) => s.Any(char.IsDigit);
    private static bool HasUpper(string s) => s.Any(char.IsUpper);
    private static bool HasLower(string s) => s.Any(char.IsLower);

    [GeneratedRegex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?`~]")]
    private static partial Regex SpecialCharRegex();
    private static bool HasSpecial(string s) => SpecialCharRegex().IsMatch(s);

    private static string GetLocalized(string key)
    {
        return LocalizationService.Instance?[key] ?? key;
    }

    // ═══════════════════════════════════════
    // 規則結果記錄
    // ═══════════════════════════════════════

    /// <summary>密碼原則規則結果</summary>
    public record PolicyRule(string Description, bool IsMet);
}
