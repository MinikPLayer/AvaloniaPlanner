using Avalonia.Controls;
using AvaloniaPlanner.ViewModels;

namespace AvaloniaPlanner.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Singleton;

        public MainWindowViewModel? ViewModel => (this.DataContext) as MainWindowViewModel;

        public MainWindow()
        {
            Singleton = this;
            InitializeComponent();
        }
    }
}