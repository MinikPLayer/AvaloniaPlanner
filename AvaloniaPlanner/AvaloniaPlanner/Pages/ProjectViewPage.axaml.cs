using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaPlanner.Controls;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.Utils;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlanner.Views;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Data;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public ObservableCollection<ProjectBinViewModel> Bins { get; set; }

        public ProjectViewViewModel(ApiProject? p = null)
        {
            if (p == null)
                p = new ApiProject() { Name = "New project" };

            project = p;

            Bins = new();
            Bins.AddRange(p.Bins.Select(b => new ProjectBinViewModel(b)));

            Bins.CollectionChanged += (s, e) => e.FillToList(project.Bins, (ProjectBinViewModel bin) => bin.GetBin());
        }
    }

    public partial class ProjectViewPage : UserControl
    {
        public void AddBinButtonClicked(object sender, RoutedEventArgs e)
        {
            var pVm = (ProjectViewViewModel)this.DataContext!;
            var bin = new ApiProjectBin() { Name = "New bin" }.Populate();

            var newBin = new ProjectBinViewModel(bin);
            newBin.BinEditCommand.Execute(null);
            pVm.Bins.Add(newBin);

            ProjectsPage.SignalProjectsChanged(newBin.GetBin().Project_id);
        }

        public void BinSearchRequested(object sender, SearchEventArgs e)
        {
            throw new NotImplementedException();   
        }

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

        public async void DeleteBinCommand(object sender, RoutedEventArgs e)
        {
            if (sender is not Control c || c.DataContext is not ProjectBinViewModel vm)
                return;

            if(await ConfirmDialog.ShowDialog())
            {
                var pVm = (ProjectViewViewModel)this.DataContext!;
                pVm.Bins.Remove(vm);
                ProjectsPage.SignalProjectsChanged(vm.GetBin().Project_id);
            }
        }

        public void TasksListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox lb || e.AddedItems.Count == 0 || e.AddedItems[0] is not ProjectTaskViewModel oldTask)
                return;

            if (lb.DataContext is not ProjectBinViewModel binVm)
            {
                Debug.WriteLine("[WARNING] Tasks listbox DataContext is not a ProjectBinViewModel type");
                return;
            }

            MainView.OpenDialog(new ProjectTaskEditDialog(oldTask.GetTask()), (s, e) =>
            {
                var result = e.Parameter;
                if (result is bool b && b == true && e.Content is ProjectTaskEditDialog dialog)
                {
                    if (dialog.DataContext is not ProjectTaskViewModel newTask)
                        throw new Exception("Dialog data context is an invalid type");

                    // ObservableCollection.Replace() is not working properly (entry disappears only to reappear after next refresh)
                    // So we need to use .Replace() twice to refresh after disappearing
                    binVm.Tasks.Replace(oldTask, newTask);
                    binVm.Tasks.Replace(newTask, newTask);
                    ProjectsPage.SignalProjectsChanged(newTask.GetTask().Project_id);
                }
            });

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
