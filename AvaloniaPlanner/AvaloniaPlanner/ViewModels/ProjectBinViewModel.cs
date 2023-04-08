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

        private int _visibleTasksCount = -1;
        public int VisibleTasksCount
        {
            get => _visibleTasksCount;
            set
            {
                if (_visibleTasksCount == value) 
                    return;
                
                _visibleTasksCount = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(VisibleTasks));
            } 
        }
        public ObservableCollection<ProjectTaskViewModel> Tasks { get; }

        public List<ProjectTaskViewModel> VisibleTasks
        {
            get
            {
                if (VisibleTasksCount == -1)
                    return Tasks.ToList();
                
                var i = 0;
                return Tasks.TakeWhile(task => i++ < VisibleTasksCount).ToList();
            }
        }
        
        public ProjectBinViewModel(ApiProjectBin bin)
        {
            Tasks = new ObservableCollection<ProjectTaskViewModel>();
            Tasks.AddRange(bin.Tasks.Select(x => new ProjectTaskViewModel(x)));
            Tasks.ConnectToList(bin.Tasks, (ProjectTaskViewModel vm) => vm.GetTask());
            Tasks.CollectionChanged += (s, e) => this.RaisePropertyChanged(nameof(VisibleTasks));
            
            this.bin = bin;
        }
    }
}
