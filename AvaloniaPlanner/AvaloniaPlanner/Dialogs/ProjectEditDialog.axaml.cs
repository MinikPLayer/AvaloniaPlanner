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
                if(DeadlineDatePicker.SelectedDate.HasValue && DeadlineTimePicker.SelectedTime.HasValue)
                    vm.Deadline = DeadlineDatePicker.SelectedDate.Value.Date + DeadlineTimePicker.SelectedTime.Value;

                ClassCopier.Copy(vm.GetProject(), ogProject);
                Save = true;
            }
            else
            {
                Save = false;
            }

            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(Save);
        }

        public ProjectEditDialog(ApiProject ogProject, bool newProject = false)
        {
            InitializeComponent();
            this.ogProject = ogProject;
            var vm = new ProjectViewModel(ClassCopier.Create<ApiProject>(ogProject), newProject);
            this.DataContext = vm;

            DeadlineDatePicker.SelectedDate = vm.Deadline;
            DeadlineTimePicker.SelectedTime = vm.Deadline.TimeOfDay;
        }
    }
}
