using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.Views;

namespace AvaloniaPlanner.Dialogs
{
    public partial class RegisterDialog : UserControl
    {
        public string? Login { get; private set; }
        public string? Password { get; private set; }
        public string? Email { get; private set; }
        public string? Username { get; private set; }

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            bool status = false;
            if (sender is Control c && c.Tag is string s && s == "Save")
            {
                Login = LoginBox.Text;
                Password = PasswordBox.Text;
                Email = EmailBox.Text;
                Username = UsernameBox.Text;
                status = true;
            }

            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(status);
        }

        public RegisterDialog()
        {
            InitializeComponent();
        }
    }
}
