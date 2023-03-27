using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.Utils;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlanner.Views;
using CSUtil.Web;

namespace AvaloniaPlanner.Pages
{
    public partial class SettingsPage : UserControl
    {

        public struct ConfigData
        {
            public string IP { get; set; }
            public short Port { get; set; }
        }

        public async void SaveServerInfoClicked(object sender, RoutedEventArgs e)
        {
            if(short.TryParse(ServerPortTextBox.Text, out short ret))
            {
                Api.port = ret;
            }
            else
            {
                await MainView.OpenDialog(new ErrorDialog("Invalid port, cannot convert to integer"));
                return;
            }
            Api.baseUrl = ServerIpTextBox.Text!;

            ((SettingsViewModel)this.DataContext!).IsLocked = true;
            var con = await SettingsSync.TestConnection();
            if(!con)
            {
                await MainView.OpenDialog(new ErrorDialog("Cannot connect to the server"));
                ((SettingsViewModel)this.DataContext).ConnectionStatus = false.ToString();
            }
            else
            {
                _ = MainView.OpenDialog(new ErrorDialog("Connected to the server"));
                ((SettingsViewModel)this.DataContext).ConnectionStatus = true.ToString();
            }

            ((SettingsViewModel)this.DataContext).IsLocked = false;
        }

        public SettingsPage()
        {
            InitializeComponent();
            this.DataContext = new SettingsViewModel();

            ServerPortTextBox.Text = Api.port.ToString();
            ServerIpTextBox.Text = Api.baseUrl;
        }
    }
}
