namespace EOM.TSHotelManagementSystem.Mobile.UI;

public interface IThemeService
{
    bool IsDarkMode { get; }
    void SetDarkMode(bool isDark);
}
