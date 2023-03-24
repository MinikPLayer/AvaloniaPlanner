using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
using System.Threading.Tasks;

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

        private SolidColorBrush binsBackground = new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0));
        public SolidColorBrush BinsBackground
        {
            get => binsBackground;
            set => this.RaiseAndSetIfChanged(ref binsBackground, value);
        }

        public void SetNormalBackground() => BinsBackground = new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0));
        public void SetHighlightedBackground() => BinsBackground = new SolidColorBrush(Color.FromArgb(64, 255, 0, 0));

        public ObservableCollection<ProjectBinViewModel> Bins { get; set; }

        public ProjectViewViewModel(ApiProject? p = null)
        {
            if (p == null)
                p = new ApiProject() { Name = "New project" };

            project = p;

            Bins = new();
            Bins.AddRange(p.Bins.Select(b => new ProjectBinViewModel(b)));
            Bins.ConnectToList(project.Bins, (ProjectBinViewModel bin) => bin.GetBin());
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

        public async void DeleteBinClicked(object sender, RoutedEventArgs e)
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

        public ProjectBinViewModel? GetBinByTask(ProjectTaskViewModel task) => ((ProjectViewViewModel)this.DataContext!).Bins.Where(b => b.Tasks.Contains(task)).FirstOrDefault();

        public async void AddTaskClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Control c || c.DataContext is not ProjectBinViewModel vm)
                return;

            var task = new ApiProjectTask() { Name = "New task" }.Populate();
            var newTask = new ProjectTaskViewModel(task);
            vm.Tasks.Add(newTask);
            ProjectsPage.SignalProjectsChanged(newTask.GetTask().Project_id);

            await EditTask(newTask, vm);
        }

        public async Task EditTask(ProjectTaskViewModel task, ProjectBinViewModel bin)
        {
            await MainView.OpenDialog(new ProjectTaskEditDialog(task.GetTask()), (s, e) =>
            {
                var result = e.Parameter;
                if (result is bool b && b == true && e.Content is ProjectTaskEditDialog dialog)
                {
                    if (dialog.DataContext is not ProjectTaskViewModel newTask)
                        throw new Exception("Dialog data context is an invalid type");

                    bin.Tasks.Replace(task, newTask);
                    ProjectsPage.SignalProjectsChanged(newTask.GetTask().Project_id);
                }
            });
        }

        public async void EditTaskClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Control c || c.DataContext is not ProjectTaskViewModel task)
                return;

            var bin = GetBinByTask(task);
            if (bin == null)
            {
                await MainView.OpenDialog(new ErrorDialog("Internal error, cannot find task's bin"));
                return;
            }

            await EditTask(task, bin);
        }

        private ProjectTaskViewModel? SelectedTaskToMove = null;
        public void MoveTaskClicked(object sender, RoutedEventArgs e)
        {
            if(sender is not Control c || c.DataContext is not ProjectTaskViewModel task)
                return;

            ((ProjectViewViewModel)this.DataContext!).SetHighlightedBackground();
            SelectedTaskToMove = task;
        }

        public async void DeleteTaskClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Control c || c.DataContext is not ProjectTaskViewModel task)
                return;

            var bin = GetBinByTask(task);
            if (bin == null)
            {
                await MainView.OpenDialog(new ErrorDialog("Internal error, cannot find task's bin"));
                return;
            }

            await MainView.OpenDialog(new ConfirmDialog(), handler: (s, e) =>
            {
                if (e.Parameter is bool b && b == true)
                    bin.Tasks.Remove(task);
            });
        }

        // Mitigate animation getting stuck in ListBox ripple effect
        public void TasksListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox lb)
                return;

            lb.SelectedItem = null;
        }        
        
        public void BinSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox lb)
                return;

            if(SelectedTaskToMove != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ProjectBinViewModel newBin)
            {
                var bin = GetBinByTask(SelectedTaskToMove);
                if (bin == null)
                {
                    _ = MainView.OpenDialog(new ErrorDialog("Internal error, cannot find task's bin"));
                    return;
                }

                bin.Tasks.Remove(SelectedTaskToMove);
                newBin.Tasks.Add(SelectedTaskToMove);
                SelectedTaskToMove = null;

                ((ProjectViewViewModel)this.DataContext!).SetNormalBackground();
            }

            lb.SelectedItem = null;
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
