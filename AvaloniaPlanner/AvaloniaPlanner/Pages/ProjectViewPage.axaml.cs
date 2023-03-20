using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Data;
using DynamicData;
using ReactiveUI;
using System;
using System.Linq;

namespace AvaloniaPlanner.Pages
{
    public class ProjectViewViewModel : ReactiveObject
    {
        private ApiProject project;

        public string ProjectName
        {
            get => project.Name;
            set
            {
                project.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public OList<ProjectBinViewModel> Bins { get; set; }

        public ProjectViewViewModel(ApiProject? p = null)
        {
            if (p == null)
                p = new ApiProject() { Name = "New project" };

            project = p;

            Bins = new();
            Bins.AddRange(p.Bins.Select(b => new ProjectBinViewModel(b)));
            Bins.OnCollectionChanged += (list) => this.RaisePropertyChanged(nameof(Bins));
        }
    }

    public partial class ProjectViewPage : UserControl
    {
        public void StatusComboBoxPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                e.Handled = true;
                foreach (var cb in this.GetVisualDescendants().OfType<ComboBox>())
                    cb.IsDropDownOpen = true;
            }

            base.OnPointerPressed(e);
        }

        public void ProjectStatusComboBoxDropDownOpened(object sender, EventArgs e)
        {
            //if (sender is not ComboBox cb)
            //    return;

            //cb.IsDropDownOpen = false;
        }

        public void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox lb)
                return;

            if (lb.SelectedItem is not ProjectTaskViewModel ptvm)
                return;

            ptvm.StatusComboBoxDropDownOpened = true;
        }

        public ProjectViewPage(ApiProject p)
        {
            this.DataContext = new ProjectViewViewModel(p);
            InitializeComponent();
        }

        public ProjectViewPage()
        {
            this.DataContext = new ProjectViewViewModel();
            InitializeComponent();
        }
    }
}
