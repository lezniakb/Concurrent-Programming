using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TP.ConcurrentProgramming.PresentationView.Converters
{
    public class MultiplyConverter : IValueConverter
    {
        // Multiplies the input value by the provided ConverterParameter.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert the input value and the parameter to doubles.
            double number = System.Convert.ToDouble(value);
            double factor = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return number * factor;
        }

        // ConvertBack is not implemented in this scenario.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
