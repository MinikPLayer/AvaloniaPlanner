using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlanner.Views;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Reflection;
using ReactiveUI;

namespace AvaloniaPlanner.Dialogs
{
    public class ConfirmDialogViewModel : ReactiveObject
    {
        private string _confirmationText = "";
        public string ConfirmationText
        {
            get => _confirmationText;
            set => this.RaiseAndSetIfChanged(ref _confirmationText, value);
        }

        public ConfirmDialogViewModel(string text)
        {
            this.ConfirmationText = text;
        }
    }

    public partial class ConfirmDialog : UserControl
    {
        public string ConfirmationText { get; set; } = "";
        public bool Result { get; set; } = false;

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Control)?.Tag as string;
            Result = tag == null ? false : tag == "yes";
            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(Result);
        }

        public ConfirmDialog(string confirmationText = "Are you sure?")
        {
            this.DataContext = new ConfirmDialogViewModel(confirmationText);
            InitializeComponent();
        }
    }
}
