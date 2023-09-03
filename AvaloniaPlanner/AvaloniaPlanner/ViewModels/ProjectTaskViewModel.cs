using AvaloniaPlanner.Controls;
using AvaloniaPlanner.Models;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Reflection;
using Material.Icons;
using Material.Icons.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.ViewModels
{
    public class ProjectTaskViewModel : ReactiveObject
    {
        private ApiProjectTask task;
        
        public void SignalModified()
        {
            LastUpdate = DateTime.Now;
        }

        public bool IsNewTask = false;

        private bool createDefaultBins = false;
        public bool CreateDefaultBins
        {
            get => createDefaultBins;
            set
            {
                createDefaultBins = value;
                this.RaisePropertyChanged();
            }
        }

        public DateTime LastUpdate
        {
            get => task.LastUpdate;
            set
            {
                if (task.LastUpdate == value)
                    return;
                
                task.LastUpdate = value;
                this.RaisePropertyChanged();
            }
        }
        
        public string TaskName
        {
            get => task.Name;
            set
            {
                if (task.Name == value)
                    return;
                
                task.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public int Priority
        {
            get => task.Priority;
            set
            {
                if (task.Priority == value)
                    return;
                
                task.Priority = value;
                this.RaisePropertyChanged();
            }
        }
        
        public string Description
        {
            get => task.Description;
            set
            {
                task.Description = value;
                this.RaisePropertyChanged();
            }
        }

        public bool DeadlineEnabled
        {
            get => task.DeadlineEnabled;
            set
            {
                task.DeadlineEnabled = value;
                this.RaisePropertyChanged();
            }
        }
        
        public DateTime Deadline
        {
            get => task.Deadline;
            set
            {
                if (task.Deadline == value)
                    return;
                
                task.Deadline = value;
                this.RaisePropertyChanged();
            }
        }

        public ProjectStatus Status => task.status;

        public ApiProjectTask GetTask() => task;

        public ProjectStatusModel StatusModel
        {
            get => ProjectStatusModel.Get(Status);
            set
            {
                if (task.status == value.Status)
                    return;
                
                task.status = value.Status;
                this.RaisePropertyChanged();
            }
        }

        public ProjectTaskViewModel(ApiProjectTask? task)
        {
            if (task == null)
                task = new ApiProjectTask();

            this.task = task;
            
        }

        public ProjectTaskViewModel() : this(null) { }
    }
}
