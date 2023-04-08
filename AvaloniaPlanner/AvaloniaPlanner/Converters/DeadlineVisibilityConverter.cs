using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib.Data.Project;

namespace AvaloniaPlanner.Converters
{
    internal class DeadlineVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ProjectStatus status;
            bool enabled;
            switch (value)
            {
                case ProjectViewModel project:
                    status = project.Status;
                    enabled = project.DeadlineEnabled;
                    break;
                case ProjectTaskViewModel task:
                    status = task.Status;
                    enabled = task.DeadlineEnabled;
                    break;
                default:
                    throw new ArgumentException("Value must be of type ProjectViewModel or ProjectTaskViewModel");
            }
            
            return !ApiProject.DisabledStatuses.Contains(status) && enabled;     
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
