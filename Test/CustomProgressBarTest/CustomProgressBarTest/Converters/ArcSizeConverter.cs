using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CustomProgressBarTest
{
    public class ArcSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && (v > 0.0))
            {
                return new Size(v / 2, v / 2);
            }

            return new Point();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
