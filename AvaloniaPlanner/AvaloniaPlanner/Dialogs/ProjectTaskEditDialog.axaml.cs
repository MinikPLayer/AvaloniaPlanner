using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlanner.Views;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Reflection;

namespace AvaloniaPlanner.Dialogs
{
    public partial class ProjectTaskEditDialog : UserControl
    {
        public bool Save { get; set; } = false;

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            if(sender is Control c)
                Save = c.Tag is string s && s == "Save";

            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(Save);
        }

        public ProjectTaskEditDialog(ApiProjectTask task)
        {
            InitializeComponent();
            this.DataContext = new ProjectTaskViewModel(task);
            
        }
    }
}
