using System;
using System.Globalization;
using System.Windows.Data;
using StarResonanceDpsAnalysis.Core.Models;

namespace StarResonanceDpsAnalysis.WPF.Converters
{
    /// <summary>
    /// Converts numeric values to compact human-readable strings.
    /// Supports runtime-changeable mode when used as a MultiBinding (number + mode).
    /// - KMB mode: "K", "M", "B"
    /// - Wan mode: "万", "亿"
    /// 
    /// Usage (single value, explicit parameter string "Wan" or "KMB"):
    ///   Text="{Binding SomeNumber, Converter={StaticResource HumanReadableNumberConverter}, ConverterParameter=Wan}"
    ///
    /// Recommended for runtime updates: pass current mode as second binding:
    ///   <TextBlock.Text>
    ///     <MultiBinding Converter="{StaticResource HumanReadableNumberConverter}">
    ///       <Binding Path="SomeNumber" />
    ///       <Binding Path="YourModeProperty" /> <!-- e.g. "Wan" or "KMB" or NumberDisplayMode enum -->
    ///     </MultiBinding>
    ///   </TextBlock.Text>
    /// When the second binding value changes, the binding will re-run and UI updates automatically.
    /// </summary>
    public class HumanReadableNumberConverter : IValueConverter, IMultiValueConverter
    {
        // IValueConverter support (keeps previous behavior; parameter can be "Wan" or "KMB")
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = ParseModeFromParameter(parameter);
            if (!TryGetDouble(value, out var d)) return string.Empty;
            return Format(d, mode, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        // IMultiValueConverter: expects values[0] = number, values[1] = mode (string or enum)
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0) return string.Empty;

            // mode override from multi-binding second value
            NumberDisplayMode mode = NumberDisplayMode.KMB;
            if (values.Length > 1 && values[1] != null)
            {
                mode = ParseModeFromObject(values[1]);
            }
            else
            {
                // fallback to parameter if provided
                mode = ParseModeFromParameter(parameter);
            }

            if (!TryGetDouble(values[0], out var d)) return string.Empty;
            return Format(d, mode, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private static NumberDisplayMode ParseModeFromParameter(object parameter)
        {
            if (parameter == null) return NumberDisplayMode.KMB;
            return ParseModeFromObject(parameter);
        }

        private static NumberDisplayMode ParseModeFromObject(object? o)
        {
            if (o == null) return NumberDisplayMode.KMB;
            switch (o)
            {
                case NumberDisplayMode nm:
                    return nm;
                case string s:
                    if (string.Equals(s, "Wan", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(s, "万", StringComparison.OrdinalIgnoreCase))
                        return NumberDisplayMode.Wan;
                    return NumberDisplayMode.KMB;
                default:
                    try
                    {
                        var txt = o.ToString();
                        if (string.Equals(txt, "Wan", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(txt, "万", StringComparison.OrdinalIgnoreCase))
                            return NumberDisplayMode.Wan;
                    }
                    catch { }
                    return NumberDisplayMode.KMB;
            }
        }

        private static string Format(double d, NumberDisplayMode mode, CultureInfo culture)
        {
            var sign = d < 0 ? "-" : "";
            d = Math.Abs(d);

            if (mode == NumberDisplayMode.Wan)
            {
                if (d >= 100_000_000) return sign + (d / 100_000_000d).ToString("0.##", culture) + "亿";
                if (d >= 10_000) return sign + (d / 10_000d).ToString("0.##", culture) + "万";
                return sign + d.ToString("0.##", culture);
            }

            // KMB
            if (d >= 1_000_000_000) return sign + (d / 1_000_000_000d).ToString("0.##", culture) + "B";
            if (d >= 1_000_000) return sign + (d / 1_000_000d).ToString("0.##", culture) + "M";
            if (d >= 1_000) return sign + (d / 1_000d).ToString("0.##", culture) + "K";
            return sign + d.ToString("0.##", culture);
        }

        private static bool TryGetDouble(object value, out double result)
        {
            switch (value)
            {
                case double dv:
                    result = dv; return true;
                case float fv:
                    result = fv; return true;
                case int iv:
                    result = iv; return true;
                case long lv:
                    result = lv; return true;
                case decimal dec:
                    result = (double)dec; return true;
                case string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                    result = parsed; return true;
                default:
                    try
                    {
                        result = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        return true;
                    }
                    catch
                    {
                        result = 0; return false;
                    }
            }
        }
    }
}