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

        private bool _statusComboBoxDropDownOpened = false;
        public bool StatusComboBoxDropDownOpened
        {
            get => _statusComboBoxDropDownOpened;
            set => this.RaiseAndSetIfChanged(ref _statusComboBoxDropDownOpened, value);
        }

        public string TaskName
        {
            get => task.Name;
            set
            {
                task.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public ProjectStatus Status => task.status;

        public ApiProjectTask GetTaskCopy() => ClassCopier.Create<ApiProjectTask>(task);

        public ProjectStatusModel StatusModel
        {
            get => ProjectStatusModel.Get(Status);
            set
            {
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
