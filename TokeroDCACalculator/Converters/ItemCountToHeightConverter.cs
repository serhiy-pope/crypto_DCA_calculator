using System.Globalization;

namespace TokeroDCACalculator.Converters
{
    public class ItemCountToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is int count)
                {
                    int itemHeight = 60;
                    int minHeight = 120;
                    int maxHeight = count * itemHeight;

                    int calculatedHeight = Math.Max(minHeight, count * itemHeight);
                    return Math.Min(calculatedHeight, maxHeight);
                }
                return 120;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Converter error: {ex.Message}");
                return 120; 
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
