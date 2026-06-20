namespace EOM.TSHotelManagementSystem.Mobile.UI;

public class ThemeService : IThemeService
{
    private const string DarkModeKey = "IsDarkMode";

    public bool IsDarkMode => Preferences.Default.Get(DarkModeKey, false);

    public void SetDarkMode(bool isDark)
    {
        Preferences.Default.Set(DarkModeKey, isDark);
        Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
    }
}
