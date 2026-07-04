using System.Globalization;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class IsNotCanceledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                return status != 2; // 2 = 已取消
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
