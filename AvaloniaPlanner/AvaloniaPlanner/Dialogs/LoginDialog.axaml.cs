using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlanner.Views;
using CSUtil.Reflection;

namespace AvaloniaPlanner.Dialogs
{
    public partial class LoginDialog : UserControl
    {
        public string? Login { get; private set; }
        public string? Password { get; private set; }

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            bool status = false;
            if (sender is Control c && c.Tag is string s && s == "Save")
            {
                Login = LoginBox.Text;
                Password = PasswordBox.Text;
                status = true;
            }

            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(status);
        }

        public LoginDialog()
        {
            InitializeComponent();
        }
    }
}
