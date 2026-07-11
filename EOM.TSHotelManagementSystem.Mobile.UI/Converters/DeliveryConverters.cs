using System.Globalization;
using Microsoft.Maui.ApplicationModel;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class BoolToBgColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            return value is true
                ? (isDark ? Color.FromArgb("#33240F") : Color.FromArgb("#F0EBFF"))
                : (isDark ? Color.FromArgb("#333333") : Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class BoolToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            return value is true
                ? (isDark ? Color.FromArgb("#FF8A65") : Color.FromArgb("#FF5722"))
                : Color.FromArgb("#B0B0B0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
