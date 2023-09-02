using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.Views;
using ReactiveUI;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Dialogs
{
    public enum SyncDialogResults
    {
        ForceUpload = 0,
        Download = 1,
        Cancel = 2
    }

    public class SyncConflictDialogViewModel : ReactiveObject
    {
        private string _message = "";
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public SyncConflictDialogViewModel(string text)
        {
            this.Message = text;
        }
    }

    public partial class SyncConflictDialog : UserControl
    {
        public SyncDialogResults Result { get; set; } = SyncDialogResults.Cancel;

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Control)?.Tag as string;
            Result = (SyncDialogResults)int.Parse(tag);
            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(Result);
        }

        public static async Task<SyncDialogResults> ShowDialog(string text)
        {
            var result = SyncDialogResults.Cancel;
            var ev = new ManualResetEvent(false);
            await MainView.OpenDialog(new SyncConflictDialog(text), (s, e) =>
            {
                if (e.Parameter is SyncDialogResults r)
                    result = r;

                ev.Set();
            });

            ev.WaitOne();
            return result;
        }

        public SyncConflictDialog(string message)
        {
            this.DataContext = new SyncConflictDialogViewModel(message);
            InitializeComponent();
        }
    }
}
