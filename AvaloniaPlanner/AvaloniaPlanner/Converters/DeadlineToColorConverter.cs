using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Converters
{
    internal class DeadlineToColorConverter : IValueConverter
    {
        static Color defaultColor = Colors.White;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is DateTime dt)
            {
                if(dt == DateTime.MaxValue)
                    return defaultColor;

                var now = DateTime.Now;
                if(now > dt)
                    return new SolidColorBrush(Colors.DarkRed);

                var diff = dt - now;
                if (diff >= TimeSpan.FromDays(7))
                    return new SolidColorBrush(defaultColor);

                if (diff >= TimeSpan.FromDays(1))
                    return new SolidColorBrush(Colors.Yellow);

                if (diff >= TimeSpan.FromHours(1))
                    return new SolidColorBrush(Colors.Orange);

                if (diff >= TimeSpan.Zero)
                    return new SolidColorBrush(Colors.Red);

                return new SolidColorBrush(Colors.DarkRed);
            }

            throw new NotSupportedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
