# TRIO2026 對話框設計規範：禁用 OS MessageBox

> 製作者：Office of William  
> 建立日期：2026-04-30  
> 版本：1.0

---

## 1. 決策摘要

**規範**：TRIO2026 專案中，所有對話框一律使用自訂 WPF Overlay Dialog，**禁止使用** `System.Windows.MessageBox`。

---

## 2. 決策原因

### 2.1 嵌入模式相容性

TRIO2026.App 透過 DevLauncher 以 Win32 `SetParent` 嵌入模擬器面板。OS `MessageBox` 是獨立的 Win32 頂層視窗，**會浮出面板外**，破壞模擬器的視覺完整性。

```
DevLauncher 視窗
├── 工具列
├── 模擬面板 (WindowsFormsHost → Panel)
│   └── TRIO2026.App (嵌入子視窗)
│       └── LoginWindow
│           └── ❌ MessageBox → 浮出面板外！
│           └── ✅ Overlay Dialog → 在 LoginWindow 內部渲染
└── 狀態列
```

### 2.2 觸控操作體驗

機台使用 7 吋觸控面板（1200×1920, 200% DPI），OS `MessageBox` 的按鈕尺寸固定且偏小，不適合手指操作。自訂 Overlay Dialog 可以設定符合觸控標準的按鈕尺寸（最小 44×44 px）。

### 2.3 視覺一致性

OS `MessageBox` 使用 Windows 系統主題（淺色），與 TRIO2026 深色毛玻璃設計風格不一致。自訂 Overlay Dialog 可完全融入應用程式的設計語言。

### 2.4 功能擴展性

| 功能 | OS MessageBox | WPF Overlay Dialog |
|------|:---:|:---:|
| 自訂按鈕文字 | ❌ | ✅ |
| 自訂圖示 | ❌ | ✅ |
| 半透明遮罩 | ❌ | ✅ |
| 進場/退場動畫 | ❌ | ✅ |
| 自動計時關閉 | ❌ | ✅ |
| 非阻斷 (async) | ❌ | ✅ |
| 嵌入模式相容 | ❌ | ✅ |

---

## 3. 實作元件

### 3.1 檔案位置

| 檔案 | 說明 |
|------|------|
| `Controls/OverlayDialog.xaml` | 對話框 UI 樣板 |
| `Controls/OverlayDialog.xaml.cs` | 對話框邏輯（顯示/關閉/回傳結果） |

### 3.2 使用方式

```csharp
// 取代 MessageBox.Show(...)
await OverlayDialog.ShowAsync(
    overlayHost: DialogOverlay,        // XAML 中的容器
    title: "TRIO2026",
    message: "登入成功！",
    buttonText: "確定",
    icon: OverlayDialogIcon.Success
);
```

---

## 4. 開發規範

1. **新增對話框時**，一律使用 `OverlayDialog.ShowAsync()`
2. **Code Review 時**，若發現 `MessageBox.Show` 應退回修改
3. **確認/取消雙按鈕對話框**，使用 `OverlayDialog.ShowConfirmAsync()`
4. **錯誤訊息**，使用 `OverlayDialog.ShowAsync()` 搭配 `OverlayDialogIcon.Error`
