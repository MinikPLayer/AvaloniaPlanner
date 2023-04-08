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
        private ApiProjectTask ogTask { get; set; }
        public bool Save { get; set; } = false;
        public bool NewTask { get; set; } = false;

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && this.DataContext is ProjectTaskViewModel vm && c.Tag is string s && s == "Save")
            {
                // Copy new data to the new task
                ClassCopier.Copy(vm.GetTask(), ogTask);
                Save = true;
            }
            else
            {
                Save = false;
            }

            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(Save);
        }

        public ProjectTaskEditDialog(ApiProjectTask task, bool isNewTask = false)
        {
            InitializeComponent();
            this.ogTask = task;
            this.DataContext = new ProjectTaskViewModel(ClassCopier.Create<ApiProjectTask>(task));
            this.NewTask = isNewTask;
        }
    }
}
