using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TRIO2026.App.Services;

namespace TRIO2026.App.Controls;

/// <summary>
/// 觸控全鍵盤覆蓋層 — LoginPage 帳號/密碼輸入用
/// 
/// 功能：
///   - QWERTY 固定排列鍵盤
///   - Shift 大小寫切換
///   - 符號模式切換（?123 / ABC）
///   - 密碼模式：遮罩顯示 + 眼睛切換
///   - 帳號模式：明碼顯示
///   - 觸控環境：52px 按鍵、無 Hover、底部對齊
/// 
/// 製作者: Office of William
/// </summary>
public partial class TouchKeyboardOverlay : UserControl
{
    // QWERTY 鍵盤佈局
    private static readonly string[][] LetterRows =
    [
        ["1","2","3","4","5","6","7","8","9","0"],
        ["q","w","e","r","t","y","u","i","o","p"],
        ["a","s","d","f","g","h","j","k","l"],
        ["z","x","c","v","b","n","m"]
    ];

    private static readonly string[][] SymbolRows =
    [
        ["!","@","#","$","%","^","&","*","(",")"],
        ["-","_","=","+","[","]","{","}","\\","|"],
        [";",":","'","\"",",",".","/","?","~"],
        ["`","<",">"]
    ];

    private string _inputText = "";
    private bool _isPasswordMode;
    private bool _showPlainText;
    private bool _isShifted;
    private bool _isSymbolMode;

    private Action<string>? _onConfirm;
    private Action? _onCancel;

    public TouchKeyboardOverlay()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 顯示鍵盤
    /// </summary>
    /// <param name="isPassword">true=密碼模式（遮罩+眼睛），false=帳號模式（明碼）</param>
    /// <param name="initialText">初始文字</param>
    /// <param name="onConfirm">確認 callback</param>
    /// <param name="onCancel">取消 callback</param>
    public void Show(bool isPassword, string initialText, Action<string> onConfirm, Action? onCancel = null)
    {
        _isPasswordMode = isPassword;
        _inputText = initialText ?? "";
        _showPlainText = !isPassword; // 帳號模式預設明碼
        _isShifted = false;
        _isSymbolMode = false;
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        // 設定標題
        var loc = LocalizationService.Instance;
        TitleText.Text = isPassword
            ? loc["TouchKeyboard.TitlePassword"]
            : loc["TouchKeyboard.TitleAccount"];

        // 眼睛按鈕：僅密碼模式
        EyeToggle.Visibility = isPassword ? Visibility.Visible : Visibility.Collapsed;
        EyeToggle.Content = _showPlainText ? "🙈" : "👁";

        UpdateDisplay();
        BuildKeyboard();

        Visibility = Visibility.Visible;
    }

    /// <summary>隱藏鍵盤</summary>
    public void Hide()
    {
        Visibility = Visibility.Collapsed;
        _inputText = "";
        _onConfirm = null;
        _onCancel = null;
    }

    // ═══════════════════════════════════════
    // 鍵盤建構
    // ═══════════════════════════════════════

    private void BuildKeyboard()
    {
        KeyboardRows.Children.Clear();
        var rows = _isSymbolMode ? SymbolRows : LetterRows;

        for (int r = 0; r < rows.Length; r++)
        {
            var rowPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 1)
            };

            // 第 4 行（index=3）：前面加 Shift 或 符號切換
            if (r == 3)
            {
                if (_isSymbolMode)
                {
                    rowPanel.Children.Add(CreateFuncButton("ABC", "#5D4037", 72, OnAbcClick));
                }
                else
                {
                    var shiftLabel = _isShifted ? "⬆ ON" : "⬆";
                    var shiftBg = _isShifted ? "#4A6A2A" : "#5D4037";
                    rowPanel.Children.Add(CreateFuncButton(shiftLabel, shiftBg, 72, OnShiftClick));
                }
            }

            // 字元按鈕
            foreach (var key in rows[r])
            {
                var display = (!_isSymbolMode && _isShifted) ? key.ToUpper() : key;
                var btn = new Button
                {
                    Content = display,
                    Style = (Style)FindResource("CharKey"),
                    Tag = key // 原始小寫
                };
                btn.Click += OnCharClick;
                rowPanel.Children.Add(btn);
            }

            // 第 4 行：後面加退格
            if (r == 3)
            {
                rowPanel.Children.Add(CreateFuncButton("⌫", "#5D4037", 72, OnBackspaceClick));
            }

            KeyboardRows.Children.Add(rowPanel);
        }

        // 底部功能列：符號切換 | 空白 | 確認
        var bottomRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };

        if (_isSymbolMode)
            bottomRow.Children.Add(CreateFuncButton("ABC", "#5D4037", 80, OnAbcClick));
        else
            bottomRow.Children.Add(CreateFuncButton("?123", "#5D4037", 80, OnSymbolClick));

        // 空白鍵
        var spaceBtn = CreateFuncButton(
            LocalizationService.Instance["TouchKeyboard.Space"],
            "#3A5278", 240, OnSpaceClick);
        spaceBtn.FontSize = 16;
        bottomRow.Children.Add(spaceBtn);

        // 確認
        bottomRow.Children.Add(CreateFuncButton(
            LocalizationService.Instance["TouchKeyboard.Confirm"],
            "#2E7D32", 100, OnConfirmClick));

        KeyboardRows.Children.Add(bottomRow);
    }

    private Button CreateFuncButton(string text, string bgColor, double width, RoutedEventHandler handler)
    {
        var brush = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(bgColor));
        var btn = new Button
        {
            Content = text,
            Style = (Style)FindResource("FuncKey"),
            Tag = brush,
            MinWidth = width
        };
        btn.Click += handler;
        return btn;
    }

    // ═══════════════════════════════════════
    // 事件處理
    // ═══════════════════════════════════════

    private void OnCharClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string key)
        {
            var ch = (!_isSymbolMode && _isShifted) ? key.ToUpper() : key;
            _inputText += ch;
            UpdateDisplay();
        }
    }

    private void OnShiftClick(object sender, RoutedEventArgs e)
    {
        _isShifted = !_isShifted;
        BuildKeyboard(); // 重建以更新大小寫顯示
    }

    private void OnSymbolClick(object sender, RoutedEventArgs e)
    {
        _isSymbolMode = true;
        _isShifted = false;
        BuildKeyboard();
    }

    private void OnAbcClick(object sender, RoutedEventArgs e)
    {
        _isSymbolMode = false;
        BuildKeyboard();
    }

    private void OnBackspaceClick(object sender, RoutedEventArgs e)
    {
        if (_inputText.Length > 0)
        {
            _inputText = _inputText[..^1];
            UpdateDisplay();
        }
    }

    private void OnSpaceClick(object sender, RoutedEventArgs e)
    {
        _inputText += " ";
        UpdateDisplay();
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        var text = _inputText;
        var callback = _onConfirm;
        Hide();
        callback?.Invoke(text);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        var cancelCallback = _onCancel;
        Hide();
        cancelCallback?.Invoke();
    }

    private void EyeToggle_Click(object sender, RoutedEventArgs e)
    {
        _showPlainText = !_showPlainText;
        EyeToggle.Content = _showPlainText ? "🙈" : "👁";
        UpdateDisplay();
    }

    // ═══════════════════════════════════════
    // 顯示更新
    // ═══════════════════════════════════════

    private void UpdateDisplay()
    {
        if (string.IsNullOrEmpty(_inputText))
        {
            InputDisplay.Text = "";
            return;
        }

        const int maxVisible = 24;

        if (_showPlainText)
        {
            InputDisplay.Text = _inputText.Length > maxVisible
                ? "…" + _inputText[^maxVisible..]
                : _inputText;
        }
        else
        {
            // 密碼遮罩
            InputDisplay.Text = _inputText.Length > maxVisible
                ? "…" + new string('●', maxVisible)
                : new string('●', _inputText.Length);
        }
    }
}
