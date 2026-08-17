using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ConvenienceStorePOS.Common;

namespace ConvenienceStorePOS.Converters
{
    public class TaxRateBadgeBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush Reduced8Brush = new(Color.FromRgb(46, 125, 50)); // Deep green
        private static readonly SolidColorBrush Standard10Brush = new(Color.FromRgb(21, 101, 192)); // Deep blue
        private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(97, 97, 97));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaxRateType taxRateType)
            {
                return taxRateType switch
                {
                    TaxRateType.Reduced8 => Reduced8Brush,
                    TaxRateType.Standard10 => Standard10Brush,
                    _ => DefaultBrush
                };
            }
            return DefaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CurrencyFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
            {
                return $"¥{d:N0}";
            }
            if (value is int i)
            {
                return $"¥{i:N0}";
            }
            return "¥0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
