using System.Globalization;

namespace TokeroDCACalculator.Converters
{
    public class MonthsCountToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is int count)
                {
                    int itemHeight = 45;
                    int minHeight = 90;
                    int maxHeight = count * itemHeight;

                    int calculatedHeight = Math.Max(minHeight, count * itemHeight);
                    return Math.Min(calculatedHeight, maxHeight);
                }
                return 135;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Converter error: {ex.Message}");
                return 135; 
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
