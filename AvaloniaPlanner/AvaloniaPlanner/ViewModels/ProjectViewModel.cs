using Avalonia.Controls;
using AvaloniaPlanner.Controls;
using AvaloniaPlanner.Models;
using AvaloniaPlanner.Pages;
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
using AvaloniaPlanner.Converters;

namespace AvaloniaPlanner.ViewModels
{
    public class ProjectViewModel : ReactiveObject
    {
        private ApiProject project { get; set; }

        public string Name
        {
            get => project.Name;
            set
            {
                project.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public string Description
        {
            get => project.Description;
            set
            {
                project.Description = value;
                this.RaisePropertyChanged();
            }
        }

        public string Owner
        {
            get => project.Owner;
            set
            {
                project.Owner = value;
                this.RaisePropertyChanged();
            }
        }

        public bool DeadlineEnabled
        {
            get => project.DeadlineEnabled;
            set
            {
                project.DeadlineEnabled = value;
                this.RaisePropertyChanged();
            }
        }

        public DateTime Deadline
        {
            get => project.Deadline;
            set
            {
                project.Deadline = value;
                this.RaisePropertyChanged();
            }
        }

        public ProjectStatus Status => project.Status;
        public ProjectStatusModel StatusModel
        {
            get => ProjectStatusModel.Get(Status);
            set
            {
                project.Status = value.Status;
                this.RaisePropertyChanged();
            }
        }

        public ICommand ProjectClickedCommand { get; set; }

        public ApiProject GetProject() => project;
        public bool IsProject(ApiProject p) => p == project;

        public ProjectViewModel(ApiProject? p = null)
        {

            if (p == null)
                p = new ApiProject();

            project = p;

            ProjectClickedCommand = ReactiveCommand.Create(() => PageManager.Navigate(new ProjectViewPage(project)));
        }

    }
}
