using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlanner.Views;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Reflection;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Dialogs
{
    public partial class ProjectEditDialog : UserControl
    {
        private ApiProject ogProject { get; set; }
        public bool Save { get; set; } = false;

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && this.DataContext is ProjectViewModel vm && c.Tag is string s && s == "Save")
            {
                // Copy new data to the new project
                ClassCopier.Copy(vm.GetProject(), ogProject);
                Save = true;
            }
            else
            {
                Save = false;
            }

            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(Save);
        }

        public ProjectEditDialog(ApiProject ogProject)
        {
            InitializeComponent();
            this.ogProject = ogProject;
            this.DataContext = new ProjectViewModel(ClassCopier.Create<ApiProject>(ogProject));
        }
    }
}
