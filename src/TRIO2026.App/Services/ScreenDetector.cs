using System.Windows;
using System.Windows.Input;

namespace TRIO2026.App.Services;

/// <summary>
/// 螢幕偵測服務 — 偵測觸控支援與螢幕解析度
/// </summary>
public static class ScreenDetector
{
    /// <summary>
    /// 偵測當前系統是否有觸控螢幕
    /// </summary>
    public static bool IsTouchSupported
    {
        get
        {
            try
            {
                return Tablet.TabletDevices.Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 取得主螢幕解析度（寬）
    /// </summary>
    public static double ScreenWidth => SystemParameters.PrimaryScreenWidth;

    /// <summary>
    /// 取得主螢幕解析度（高）
    /// </summary>
    public static double ScreenHeight => SystemParameters.PrimaryScreenHeight;

    /// <summary>
    /// 取得建議的 UI 縮放因子（以 1920×1200 為基準）
    /// </summary>
    public static double ScaleFactor
    {
        get
        {
            const double baseWidth = 1920.0;
            const double baseHeight = 1200.0;
            var scaleX = ScreenWidth / baseWidth;
            var scaleY = ScreenHeight / baseHeight;
            return Math.Min(scaleX, scaleY); // 取較小值避免溢出
        }
    }
}
