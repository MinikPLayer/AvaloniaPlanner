using Avalonia.Controls;
using AvaloniaPlanner.ViewModels;

namespace AvaloniaPlanner.Pages
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.DataContext = new SettingsViewModel();
        }
    }
}
