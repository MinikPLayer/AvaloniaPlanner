using Avalonia.Controls;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib.Data.Project;

namespace AvaloniaPlanner.Controls
{
    public partial class ProjectControl : UserControl
    {
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
