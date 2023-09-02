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
    public class StringToBrushConverter : IValueConverter
    {
        public static Color TrueColor = Colors.LightGreen;
        public static Color FalseColor = Colors.Red;

        public object Convert(object? value, Type targetType)
        {
            if(targetType == typeof(string) && value is SolidColorBrush c)
            {
                if (c.Color == TrueColor)
                    return "True";
                else if (c.Color == FalseColor)
                    return "False";
                else
                    return "";
            }
            else if(targetType == typeof(IBrush) && value is string s)
            {
                var color = Colors.White;
                if (s == "True")
                    color = TrueColor;
                else if (s == "False")
                    color = FalseColor;
                else
                    color = Colors.White;

                return new SolidColorBrush(color);
            }
            else
            {
                var typeStr = value == null ? "null" : value.GetType().ToString();
                throw new NotImplementedException("Converting from " + typeStr + " to " + targetType + " is not supported");
            }
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => Convert(value, targetType);
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Convert(value, targetType);
    }
}
