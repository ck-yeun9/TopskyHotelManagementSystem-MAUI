using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    /// <summary>
    /// 消费记录时间范围切换按钮的背景色：选中=品牌橙，未选中=主题感知的浅/深灰。
    /// 绑定源为 HistoryRange，ConverterParameter 为该按钮代表的范围值（1m/3m/1y/all）。
    /// </summary>
    public class RangeBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == parameter?.ToString())
                return Color.FromArgb("#FF5722");

            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            return isDark ? Color.FromArgb("#333333") : Color.FromArgb("#F3F4F6");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// 消费记录时间范围切换按钮的文字色：选中=白，未选中=主题感知的次级灰。
    /// </summary>
    public class RangeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == parameter?.ToString())
                return Colors.White;

            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            return isDark ? Color.FromArgb("#B0B0B0") : Color.FromArgb("#6B7280");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
