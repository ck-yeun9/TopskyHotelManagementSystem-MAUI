using System.Globalization;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class NotNullOrEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string str && !string.IsNullOrEmpty(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
