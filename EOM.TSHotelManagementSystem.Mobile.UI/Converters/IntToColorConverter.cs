using System.Globalization;

namespace EOM.TSHotelManagementSystem.Mobile.UI;

public class IntToColorConverter : IValueConverter
{
    public string ZeroColor { get; set; } = "#DC2626";
    public string PositiveColor { get; set; } = "#15803D";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0 ? Color.FromArgb(PositiveColor) : Color.FromArgb(ZeroColor);
        }
        return Color.FromArgb(ZeroColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
