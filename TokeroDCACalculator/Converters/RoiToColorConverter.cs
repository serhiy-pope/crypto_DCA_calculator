using System.Globalization;

namespace TokeroDCACalculator.Converters
{
    public class RoiToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal roi)
            {
                return roi >= 0 ? Colors.LimeGreen : Colors.OrangeRed;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
