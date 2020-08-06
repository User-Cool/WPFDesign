using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CustomProgressBarTest
{
    /// <summary>
    /// Radius 。类型是 double
    /// </summary>
    class RadiusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double original1 = values[0] is double v1 ? v1 : default;
            double original2 = values[1] is double v2 ? v2 : default;
            double result = Math.Min(original1, original2);

            if (targetType == typeof(double))
                return result / 2.0;
            else
                return new CornerRadius(result / 2.0);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
