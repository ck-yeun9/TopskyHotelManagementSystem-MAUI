namespace EOM.TSHotelManagementSystem.Mobile.UI;

public interface IThemeService
{
    bool IsDarkMode { get; }
    void SetDarkMode(bool isDark);

    /// <summary>
    /// 读取已保存的主题偏好并应用到应用（用于 App 启动时恢复上次的深/浅色选择）。
    /// </summary>
    void ApplyTheme();
}
