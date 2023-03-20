using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.Views;
using ReactiveUI;

namespace AvaloniaPlanner.Dialogs
{
    public class ErrorDialogViewModel : ReactiveObject
    {
        private string _message = "";
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

    }

    public partial class ErrorDialog : UserControl
    {
        public void CloseButtonClicked(object sender, RoutedEventArgs e) => MainView.Singleton.MainDialog.CloseDialogCommand.Execute(null);

        public ErrorDialog(string message)
        {
            this.DataContext = new ErrorDialogViewModel { Message = message };
            InitializeComponent();
        }
    }
}
