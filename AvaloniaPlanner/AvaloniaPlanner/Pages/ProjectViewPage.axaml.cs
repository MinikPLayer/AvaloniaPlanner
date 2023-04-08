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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPlanner.Models;
using Material.Icons;
using Material.Icons.Avalonia;

namespace AvaloniaPlanner.Pages
{
    public class ProjectViewViewModel : ReactiveObject
    {
        private ApiProject project;

        public IEnumerable<OrderSelection> Options { get; } = new ObservableCollection<OrderSelection>()
        {
            new("Priority", MaterialIconKind.PriorityHigh, TaskOrderingModes.Priority),
            new("Name", MaterialIconKind.SortByAlpha, TaskOrderingModes.Name),
            new("Status", MaterialIconKind.CheckCircle, TaskOrderingModes.Status),
            new("Last update", MaterialIconKind.Update, TaskOrderingModes.LastUpdate)
        };

        public string ProjectName
        {
            get => project.Name;
            set
            {
                project.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public ApiProject GetProject() => project;

        private SolidColorBrush binsBackground = new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0));
        public SolidColorBrush BinsBackground
        {
            get => binsBackground;
            set => this.RaiseAndSetIfChanged(ref binsBackground, value);
        }

        public void SetNormalBackground() => BinsBackground = new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0));
        public void SetHighlightedBackground() => BinsBackground = new SolidColorBrush(Color.FromArgb(64, 255, 0, 0));

        public ObservableCollection<ProjectBinViewModel> VisibleBins { get; }
        public ObservableCollection<ProjectBinViewModel> Bins { get; }

        public void SetOrderingMode(TaskOrderingModes method, bool asc)
        {
            foreach (var bin in Bins)
                bin.Reorder(method, asc);
            

            SettingsPage.Config.TasksOrderMode = method;
            SettingsPage.Config.TasksOrderAscending = asc;
        }
        
        public void ExpandAllBins()
        {
            foreach (var bin in Bins)
                bin.Expand();
        }

        public void CollapseAllBins()
        {
            foreach (var bin in Bins)
                bin.Collapse();
        }

        public ProjectBinViewModel? GetNextBin(ProjectBinViewModel bin)
        {
            var binIndex = Bins.IndexOf(bin);
            if (binIndex == Bins.Count - 1)
                return null;

            return Bins[binIndex + 1];
        }
        
        public ProjectBinViewModel? GetPrevBin(ProjectBinViewModel bin)
        {
            var binIndex = Bins.IndexOf(bin);
            if (binIndex == 0)
                return null;

            return Bins[binIndex - 1];
        }
        
        public ProjectViewViewModel(ApiProject? p = null)
        {
            if (p == null)
                p = new ApiProject() { Name = "New project" };

            project = p;

            Bins = new();
            VisibleBins = new();
            Bins.ConnectToList(VisibleBins, x => x);

            Bins.AddRange(p.Bins.Select(b => new ProjectBinViewModel(b)));
            Bins.ConnectToList(project.Bins, (ProjectBinViewModel bin) => bin.GetBin());
        }
    }

    public partial class ProjectViewPage : UserControl
    {
        async Task EditBin(ProjectBinViewModel bin)
        {
            await MainView.OpenDialog(new ProjectBinEditDialog(bin.GetBin()), async (s, e) =>
            {
                var result = e.Parameter;
                if (result is bool b && b == true && e.Content is ProjectBinEditDialog dialog)
                {
                    if (dialog.DataContext is not ProjectBinViewModel newBin)
                        throw new Exception("Dialog data context is an invalid type");

                    var vm = (this.DataContext as ProjectViewViewModel)!;
                    vm.Bins.Replace(bin, newBin);
                    
                    ProjectsPage.SignalProjectsChanged(newBin.GetBin().Project_id);
                }
            });
        }
        
        void EditBinClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Control { DataContext: ProjectBinViewModel vm })
                return;

            _ = EditBin(vm);
        }
        
        void AddBinButtonClicked(object sender, RoutedEventArgs e)
        {
            var pVm = (ProjectViewViewModel)this.DataContext!;
            var bin = new ApiProjectBin() { Name = "New bin" }.Populate();

            var newBin = new ProjectBinViewModel(bin);
            _ = EditBin(newBin);
            pVm.Bins.Add(newBin);

            ProjectsPage.SignalProjectsChanged(newBin.GetBin().Project_id);
        }

        void BinSearchRequested(object sender, SearchEventArgs e)
        {
            var pVm = (ProjectViewViewModel)this.DataContext!;
            pVm.VisibleBins.Clear();

            if (string.IsNullOrEmpty(e.SearchTerm))
            {
                pVm.VisibleBins.AddRange(pVm.Bins);
            }
            else
            {
                var term = e.SearchTerm.ToLower();
                pVm.VisibleBins.AddRange(pVm.Bins.Where(x => x.BinName.ToLower().Contains(term) || x.Tasks.Any(x => x.TaskName.ToLower().Contains(term))));
            }
        }

        async void DeleteBinClicked(object sender, RoutedEventArgs e)
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

        ProjectBinViewModel? GetBinByTask(ProjectTaskViewModel task) => ((ProjectViewViewModel)this.DataContext!).Bins.Where(b => b.Tasks.Contains(task)).FirstOrDefault();

        async void AddTaskClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Control c || c.DataContext is not ProjectBinViewModel vm)
                return;

            var task = new ApiProjectTask() { Name = "New task" }.Populate();
            var newTask = new ProjectTaskViewModel(task);
            if (vm.DefaultStatusEnabled)
                newTask.StatusModel = vm.DefaultStatusModel;
            
            vm.Tasks.Add(newTask);
            SignalTaskModified(newTask, false);
            this.OrderSelector.ForceReorder();
            ProjectsPage.SignalProjectsChanged(newTask.GetTask().Project_id);

            await EditTask(newTask, vm, true);
        }

        async Task EditTask(ProjectTaskViewModel task, ProjectBinViewModel bin, bool isNewTask = false)
        {
            await MainView.OpenDialog(new ProjectTaskEditDialog(task.GetTask(), isNewTask), (s, e) =>
            {
                var result = e.Parameter;
                if (result is not bool b || e.Content is not ProjectTaskEditDialog { DataContext: ProjectTaskViewModel newTask } dialog) 
                    return;
                
                if (b)
                {
                    bin.Tasks.Replace(task, newTask);
                    SignalTaskModified(newTask, false);
                    this.OrderSelector.ForceReorder();
                    ProjectsPage.SignalProjectsChanged(newTask.GetTask().Project_id);
                }
                else if (dialog.NewTask)
                {
                    ForceDeleteTask(task);
                }
            });
        }

        async void EditTaskClicked(object sender, RoutedEventArgs e)
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
        void MoveTaskClicked(object sender, RoutedEventArgs e)
        {
            if(sender is not Control c || c.DataContext is not ProjectTaskViewModel task)
                return;

            ((ProjectViewViewModel)this.DataContext!).SetHighlightedBackground();
            SelectedTaskToMove = task;
        }

        void ForceDeleteTask(ProjectTaskViewModel task)
        {
            var bin = GetBinByTask(task);
            if (bin == null)
                throw new Exception("Internal error, cannot find task's bin");

            bin.Tasks.Remove(task);
            SignalTaskModified(task, false);
            this.OrderSelector.ForceReorder();
            ProjectsPage.SignalProjectsChanged(task.GetTask().Project_id);
        }
        
        async void DeleteTaskClicked(object sender, RoutedEventArgs e)
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
                if (e.Parameter is true)
                    ForceDeleteTask(task);
            });
        }

        // Mitigate animation getting stuck in ListBox ripple effect
        void TasksListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox lb)
                return;

            lb.SelectedItem = null;
        }

        void MoveTaskToBin(ProjectTaskViewModel task, ProjectBinViewModel targetBin)
        {
            var bin = GetBinByTask(task);
            if (bin == null)
            {
                _ = MainView.OpenDialog(new ErrorDialog("Internal error, cannot find task's bin"));
                return;
            }

            bin.Tasks.Remove(task);
            targetBin.Tasks.Add(task);
            if (targetBin.DefaultStatusEnabled)
                task.StatusModel = targetBin.DefaultStatusModel;
                
            SignalTaskModified(task, false);
            this.OrderSelector.ForceReorder();
            ProjectsPage.SignalProjectsChanged(task.GetTask().Project_id);
        }
        
        void BinSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox lb)
                return;

            if(SelectedTaskToMove != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ProjectBinViewModel newBin)
            {
                MoveTaskToBin(SelectedTaskToMove, newBin);
                SelectedTaskToMove = null;
                ((ProjectViewViewModel)this.DataContext!).SetNormalBackground();
            }

            lb.SelectedItem = null;
        }

        public ProjectViewPage(ApiProject? p)
        {
            var vm = new ProjectViewViewModel(p);
            this.DataContext = vm;
            InitializeComponent();

            this.OrderSelector.SetOrderMethods(vm.Options, SettingsPage.Config.TasksOrderMode, SettingsPage.Config.TasksOrderAscending);
        }

        private void SignalTaskModified(ProjectTaskViewModel obj, bool reorder)
        {
            obj.SignalModified();
            if(reorder && this.OrderSelector.GetOrderMethod() is TaskOrderingModes.LastUpdate)
                this.OrderSelector.ForceReorder();
        }

        public ProjectViewPage() : this(null) {}

        private void OrderSelector_OnOrderMethodChanged(object? sender, OrderSelectorEventArgs e)
        {
            if(e.Method is not TaskOrderingModes mode)
                return;
            
            ((ProjectViewViewModel)this.DataContext!).SetOrderingMode(mode, e.Ascending);
        }

        private void TaskLifecycleChangeClicked(object? sender, RoutedEventArgs e)
        {
            if (sender is not Control { Tag: string s, DataContext: ProjectTaskViewModel vm })
                return;

            switch (s)
            {
                case "next":
                    vm.StatusModel = vm.StatusModel.Next();
                    break;
                
                case "prev":
                    vm.StatusModel = vm.StatusModel.Prev();
                    break;
                
                default:
                    return;
            }
            SignalTaskModified(vm, false);
            this.OrderSelector.ForceReorder();
            ProjectsPage.SignalProjectsChanged(vm.GetTask().Project_id);
        }
        
        private void TaskPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not ProjectTaskControl { DataContext: ProjectTaskViewModel vm })
                return;
            
            var props = e.GetCurrentPoint(null).Properties;
            if ((e.ClickCount == 2 && props.IsLeftButtonPressed) || props.IsMiddleButtonPressed)
            {
                var lVm = (this.DataContext as ProjectViewViewModel)!;
                var bin = GetBinByTask(vm);
                if (bin == null)
                    return;

                var targetBin = props.IsMiddleButtonPressed ? lVm.GetPrevBin(bin) : lVm.GetNextBin(bin);
                if (targetBin == null)
                    return;
                
                MoveTaskToBin(vm, targetBin);
                ProjectsPage.SignalProjectsChanged(vm.GetTask().Project_id);
            }
            else if (props.IsXButton2Pressed)
            {
                vm.StatusModel = vm.StatusModel.Next();
                SignalTaskModified(vm, false);
                this.OrderSelector.ForceReorder();
                ProjectsPage.SignalProjectsChanged(vm.GetTask().Project_id);
            }
            else if (props.IsXButton1Pressed)
            {
                vm.StatusModel = vm.StatusModel.Prev();
                this.OrderSelector.ForceReorder();
                SignalTaskModified(vm, false);
                ProjectsPage.SignalProjectsChanged(vm.GetTask().Project_id);
            }
        }

        private void ExpandAllBins_Clicked(object? sender, RoutedEventArgs e)
        {
            if (sender is not Control c || c.DataContext is not ProjectViewViewModel vm)
                return;

            vm.ExpandAllBins();
        }
        
        private void CollapseAllBins_Clicked(object? sender, RoutedEventArgs e)
        {
            if (sender is not Control c || c.DataContext is not ProjectViewViewModel vm)
                return;

            vm.CollapseAllBins();
        }
    }
}
