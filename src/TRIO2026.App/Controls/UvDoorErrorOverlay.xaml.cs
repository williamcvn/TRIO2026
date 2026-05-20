using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TRIO2026.App.Controls;

/// <summary>
/// UV 門板開啟錯誤警示 Overlay
/// 
/// 使用方式：
///   Show() — 門板開啟時顯示（淡入動畫）
///   Hide() — 門板關閉時隱藏（淡出動畫）
///   無按鈕，完全由程式邏輯控制顯示/隱藏
/// </summary>
public partial class UvDoorErrorOverlay : UserControl
{
    public UvDoorErrorOverlay()
    {
        InitializeComponent();
    }

    /// <summary>顯示警示 Overlay（淡入動畫）</summary>
    public void Show()
    {
        Visibility = Visibility.Visible;
        var storyboard = (Storyboard)FindResource("FadeIn");
        storyboard.Begin(this);
    }

    /// <summary>隱藏警示 Overlay（淡出動畫後隱藏）</summary>
    public void Hide()
    {
        var storyboard = (Storyboard)FindResource("FadeOut");
        storyboard.Completed += (_, _) => Visibility = Visibility.Collapsed;
        storyboard.Begin(this);
    }
}
