namespace EOM.TSHotelManagementSystem.Mobile.UI;

public class ThemeService : IThemeService
{
    private const string DarkModeKey = "IsDarkMode";

    public bool IsDarkMode => Preferences.Default.Get(DarkModeKey, false);

    public void SetDarkMode(bool isDark)
    {
        Preferences.Default.Set(DarkModeKey, isDark);
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
        }
    }

    public void ApplyTheme()
    {
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }
    }
}
