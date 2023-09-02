using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaPlanner.Views;
using Material.Icons;
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

        private MaterialIconKind _icon = MaterialIconKind.Error;
        public MaterialIconKind Icon
        {
            get => _icon;
            set => this.RaiseAndSetIfChanged(ref _icon, value);
        }

        private SolidColorBrush _iconBrush = new SolidColorBrush(Colors.Red);
        public SolidColorBrush IconBrush
        {
            get => _iconBrush;
            set => this.RaiseAndSetIfChanged(ref _iconBrush, value);
        }
    }

    public partial class ErrorDialog : UserControl
    {
        public void CloseButtonClicked(object sender, RoutedEventArgs e) => MainView.Singleton.MainDialog.CloseDialogCommand.Execute(null);

        public ErrorDialog(string message, MaterialIconKind icon = MaterialIconKind.Error, Color? iconColor = null)
        {
            if(!iconColor.HasValue)
                iconColor = Colors.Red;

            this.DataContext = new ErrorDialogViewModel { Message = message, Icon = icon, IconBrush = new SolidColorBrush(iconColor.Value) };
            InitializeComponent();
        }
    }
}
