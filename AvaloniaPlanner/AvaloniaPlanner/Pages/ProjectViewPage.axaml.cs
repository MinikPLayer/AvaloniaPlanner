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
using AvaloniaPlanner.Models;
using Material.Icons;
using Material.Icons.Avalonia;

namespace AvaloniaPlanner.Pages
{
    public enum TaskOrderingModes
    {
        Priority,
        Name,
        Status
    }
    
    public class ProjectViewViewModel : ReactiveObject
    {
        private ApiProject project;

        public IEnumerable<OrderSelection> Options { get; } = new ObservableCollection<OrderSelection>()
        {
            new("Priority", MaterialIconKind.PriorityHigh, TaskOrderingModes.Priority),
            new("Name", MaterialIconKind.SortByAlpha, TaskOrderingModes.Name),
            new("Status", MaterialIconKind.CheckCircle, TaskOrderingModes.Status)
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
            Func<ProjectTaskViewModel, object> selector;
            switch (method)
            {
                case TaskOrderingModes.Name:
                    selector = x => x.TaskName;
                    break;
                
                case TaskOrderingModes.Priority:
                    selector = x => x.Priority;
                    break;
                    
                case TaskOrderingModes.Status:
                    selector = x => x.Status;
                    break;
                    
                default:
                    return;
            }
            
            foreach (var bin in Bins)
            {
                var newTasks = (asc ? bin.Tasks.OrderBy(selector) : bin.Tasks.OrderByDescending(selector)).ToList();
                bin.Tasks.Clear();
                bin.Tasks.AddRange(newTasks);
            }

            SettingsPage.Config.TasksOrderMode = method;
            SettingsPage.Config.TasksOrderAscending = asc;
        }
        
        public ProjectViewViewModel(ApiProject? p = null)
        {
            if (p == null)
                p = new ApiProject() { Name = "New project" };

            project = p;

            Bins = new();
            VisibleBins = new();
            Bins.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    VisibleBins.AddRange(e.NewItems.OfType<ProjectBinViewModel>());

                if (e.OldItems != null)
                    foreach (var item in e.OldItems.OfType<ProjectBinViewModel>())
                        VisibleBins.Remove(item);
            };

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
            this.OrderSelector.ForceReorder();
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
                    this.OrderSelector.ForceReorder();
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
                {
                    bin.Tasks.Remove(task);
                    this.OrderSelector.ForceReorder();
                    ProjectsPage.SignalProjectsChanged(task.GetTask().Project_id);
                }
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
                this.OrderSelector.ForceReorder();
                ProjectsPage.SignalProjectsChanged(SelectedTaskToMove.GetTask().Project_id);
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
            this.OrderSelector.ForceReorder();
            ProjectsPage.SignalProjectsChanged(vm.GetTask().Project_id);
        }
        
        private void TaskPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not ProjectTaskControl { DataContext: ProjectTaskViewModel vm })
                return;
            
            var props = e.GetCurrentPoint(null).Properties;
            if (props.IsXButton2Pressed)
            {
                vm.StatusModel = vm.StatusModel.Next();
                this.OrderSelector.ForceReorder();
                ProjectsPage.SignalProjectsChanged(vm.GetTask().Project_id);
            }
            else if (props.IsXButton1Pressed)
            {
                vm.StatusModel = vm.StatusModel.Prev();
                this.OrderSelector.ForceReorder();
                ProjectsPage.SignalProjectsChanged(vm.GetTask().Project_id);
            }
        }
    }
}
