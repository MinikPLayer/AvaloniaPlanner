using AvaloniaPlanner.Pages;
using AvaloniaPlanner.Utils;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Data;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AvaloniaPlanner.Models;
using Material.Icons;

namespace AvaloniaPlanner.ViewModels
{
    public class ProjectBinViewModel : ReactiveObject
    {
        private ApiProjectBin bin;

        public bool CustomOrderingOverrideEnabled
        {
            get => bin.CustomOrderingOverrideEnabled;
            set
            {
                bin.CustomOrderingOverrideEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        public TaskOrderingModes CustomOrderingOverride
        {
            get => bin.CustomOrderingOverride;
            set
            {
                bin.CustomOrderingOverride = value;
                this.RaisePropertyChanged();
            }
        }
        
        public string Id
        {
            get => bin.Id;
            set
            {
                bin.Id = value;
                this.RaisePropertyChanged();
            }
        }

        public string BinName
        {
            get => bin.Name;
            set
            {
                bin.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public bool DefaultStatusEnabled
        {
            get => bin.DefaultStatusEnabled;
            set
            {
                bin.DefaultStatusEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        public MaterialIconKind DefaultStatusIcon => DefaultStatusModel.Icon;
        
        public ProjectStatusModel DefaultStatusModel
        {
            get => ProjectStatusModel.Get(bin.DefaultStatus);
            set
            {
                bin.DefaultStatus = value.Status;
                this.RaisePropertyChanged();
            }
        }
        
        public ApiProjectBin GetBin() => bin;

        public int TaskCountToShow
        {
            get => bin.ShownTaskCount;
            set
            {
                if (bin.ShownTaskCount == value) 
                    return;
                
                bin.ShownTaskCount = value;
                _taskShowingCount = value;
                this.RaisePropertyChanged();
                UpdateShownTasks();
            } 
        }
        private int _taskShowingCount;

        private void UpdateShownTasks()
        {
            this.RaisePropertyChanged(nameof(CollapseButtonText));
            this.RaisePropertyChanged(nameof(VisibleTasks));
            this.RaisePropertyChanged(nameof(CollapseButtonVisible));
        }
        
        public bool CollapseButtonVisible => TaskCountToShow != -1 && TaskCountToShow < Tasks.Count;

        public string CollapseButtonText
        {
            get
            {
                if (_taskShowingCount == -1)
                    return "Collapse";

                return (Tasks.Count - _taskShowingCount) + " more...";
            }
        }

        public ICommand CollapseButtonCommand { get; }

        public ObservableCollection<ProjectTaskViewModel> Tasks { get; }

        public List<ProjectTaskViewModel> VisibleTasks
        {
            get
            {
                if (_taskShowingCount == -1)
                    return Tasks.ToList();
                
                var i = 0;
                return Tasks.TakeWhile(task => i++ < _taskShowingCount).ToList();
            }
        }

        private Func<ProjectTaskViewModel, object>? GetOrderingSelector(TaskOrderingModes method)
        {
            return method switch
            {
                TaskOrderingModes.Name => x => x.TaskName,
                TaskOrderingModes.Priority => x => x.Priority,
                TaskOrderingModes.Status => x => x.Status,
                TaskOrderingModes.LastUpdate => x => x.LastUpdate,
                _ => null
            };
        }
        
        public void Reorder(TaskOrderingModes method, bool asc)
        {
            if (CustomOrderingOverrideEnabled)
                method = CustomOrderingOverride;
            
            var selector = GetOrderingSelector(method);
            if (selector == null)
                return;
            
            var newTasks = (asc ? Tasks.OrderBy(selector) : Tasks.OrderByDescending(selector)).ToList();
            Tasks.Clear();
            Tasks.AddRange(newTasks);
        }
        
        public void Collapse()
        {
            _taskShowingCount = TaskCountToShow;
            UpdateShownTasks();
        }

        public void Expand()
        {
            _taskShowingCount = -1;
            UpdateShownTasks();
        }
        
        public ProjectBinViewModel(ApiProjectBin bin)
        {
            this.bin = bin;
            CollapseButtonCommand = ReactiveCommand.Create(() =>
            {
                if (_taskShowingCount == -1)
                    Collapse();
                else
                    Expand();
            });
            
            Tasks = new ObservableCollection<ProjectTaskViewModel>();
            Tasks.AddRange(bin.Tasks.Select(x => new ProjectTaskViewModel(x)));
            Tasks.ConnectToList(bin.Tasks, (ProjectTaskViewModel vm) => vm.GetTask());
            Tasks.CollectionChanged += (s, e) => UpdateShownTasks();

            _taskShowingCount = TaskCountToShow;
            UpdateShownTasks();
        }
    }
}
