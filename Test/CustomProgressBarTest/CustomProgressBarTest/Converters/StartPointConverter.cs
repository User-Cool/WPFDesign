using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CustomProgressBarTest
{
    public class StartPointConverter : IValueConverter
    {
        [Obsolete]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && (v > 0.0))
            {
                return new Point(v / 2, 0);
            }

            return new Point();
        }

        [Obsolete]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

    }
}
