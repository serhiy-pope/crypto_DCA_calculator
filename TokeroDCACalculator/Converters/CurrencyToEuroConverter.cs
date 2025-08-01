using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokeroDCACalculator.Converters
{
    public class CurrencyToEuroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                // Use German culture for Euro formatting (€ symbol)
                var euroCulture = CultureInfo.GetCultureInfo("de-DE");
                return decimalValue.ToString("C2", euroCulture);
            }

            if (value is double doubleValue)
            {
                var euroCulture = CultureInfo.GetCultureInfo("de-DE");
                return doubleValue.ToString("C2", euroCulture);
            }

            return "€0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
