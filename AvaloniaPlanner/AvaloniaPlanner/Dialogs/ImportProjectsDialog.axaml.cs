using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.Views;
using ReactiveUI;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Dialogs
{
    public enum ImportProjectsDialogResults
    {
        OpenSelected = 0,
        OverwriteLocal = 1,
        Cancel = 2
    }

    public class ImportProjectsDialogViewModel : ReactiveObject
    {
        private string _message = "";
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public ImportProjectsDialogViewModel(string text)
        {
            this.Message = text;
        }
    }

    public partial class ImportProjectsDialog : UserControl
    {
        private const string DefaultMessage =
            @"Do you want to override local projects with selected projects file or to open the new file in it's original location?";
        
        public ImportProjectsDialogResults Result { get; set; } = ImportProjectsDialogResults.Cancel;

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Control)?.Tag as string;
            Result = (ImportProjectsDialogResults)int.Parse(tag);
            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(Result);
        }

        public static async Task<SyncDialogResults> ShowDialog(string text)
        {
            var result = SyncDialogResults.Cancel;
            var ev = new ManualResetEvent(false);
            await MainView.OpenDialog(new ImportProjectsDialog(text), (s, e) =>
            {
                if (e.Parameter is SyncDialogResults r)
                    result = r;

                ev.Set();
            });

            ev.WaitOne();
            return result;
        }

        public ImportProjectsDialog(string message = DefaultMessage)
        {
            this.DataContext = new ImportProjectsDialogViewModel(message);
            InitializeComponent();
        }
    }
}
