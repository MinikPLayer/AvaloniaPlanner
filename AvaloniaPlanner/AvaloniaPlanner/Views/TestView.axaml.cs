using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaPlanner.Views
{
    public partial class TestView : UserControl
    {
        public async void TestClick2(object sender, RoutedEventArgs args)
        {
            var contents = new TextBlock() { Text = "TEST" };
            await MainView.OpenDialog(contents);
        }

        public TestView()
        {
            InitializeComponent();
        }
    }
}
