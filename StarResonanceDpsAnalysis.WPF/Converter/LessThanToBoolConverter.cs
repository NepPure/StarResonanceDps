using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StarResonanceDpsAnalysis.WPF.Converter
{
    public class LessThanToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d) return DoubleConverter(d, parameter);
            return false;
        }

        private static bool DoubleConverter(double value, object parameter) 
        {
            return double.TryParse(parameter?.ToString(), out var th) && value < th;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
