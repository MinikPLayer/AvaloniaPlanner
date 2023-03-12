using Avalonia.Controls;
using Avalonia.Interactivity;
using DialogHostAvalonia;

namespace AvaloniaPlanner.Views
{
    public partial class TestView : UserControl
    {
        public async void TestClick2(object sender, RoutedEventArgs args)
        {
            var contents = new TextBlock() { Text = "TEST" };
            await DialogHost.Show(contents, "TestDialog");
        }

        public TestView()
        {
            InitializeComponent();
        }
    }
}
