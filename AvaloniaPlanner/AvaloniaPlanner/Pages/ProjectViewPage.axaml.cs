using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Data;
using DialogHostAvalonia;
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

        private static bool dialogOpened = false;
        private void DialogClosed(object sender, DialogClosingEventArgs e)
        {
            dialogOpened = false;
            var result = e.Parameter;
            if (result is bool b && b == true && e.Session.Content is ProjectTaskEditDialog dialog)
            {
                this.DataContext = dialog.DataContext;
                ProjectsPage.SignalProjectsChanged();
            }
        }

        public void TasksListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            if (e.AddedItems[0] is not ProjectTaskViewModel vm)
                return;

            if (dialogOpened)
                return;

            dialogOpened = true;
            DialogHost.Show(new ProjectTaskEditDialog(vm.GetTask()), closingEventHandler: DialogClosed);

            if (sender is ListBox lb)
                lb.SelectedItem = null;
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
