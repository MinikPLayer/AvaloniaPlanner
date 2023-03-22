using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.Pages;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib.Data.Project;

namespace AvaloniaPlanner.Controls
{
    public partial class ProjectControl : UserControl
    {
        public void EditProjectClick(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is not ProjectViewModel vm)
                return;

            ProjectsPage.EditProject(vm.GetProject());
        }

        public void RemoveProjectClick(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is not ProjectViewModel vm)
                return;

            ProjectsPage.RemoveProject(vm.GetProject());
        }

        public bool IsProject(ApiProject p)
        {
            if (this.DataContext is not ProjectViewModel vm)
                return false;

            return vm.IsProject(p);
        }

        public ProjectControl(ApiProject? project = null)
        {
            InitializeComponent();
            this.DataContext = new ProjectViewModel(project);
        }
    }
}
