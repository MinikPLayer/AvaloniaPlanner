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

        private string _tempBinName = "";
        public string TempBinName
        {
            get => _tempBinName;
            set => this.RaiseAndSetIfChanged(ref _tempBinName, value);
        }


        private bool _inEditMode = false;
        public bool InEditMode
        {
            get => _inEditMode;
            set => this.RaiseAndSetIfChanged(ref _inEditMode, value);
        }

        public ApiProjectBin GetBin() => bin;

        public ICommand BinEditCommand { get; set; }
        public ICommand BinEditCancelCommand { get; set; }
        public ICommand AddTaskCommand { get; set; }

        public ObservableCollection<ProjectTaskViewModel> Tasks { get; set; }

        public ProjectBinViewModel(ApiProjectBin bin)
        {
            BinEditCommand = ReactiveCommand.Create(() =>
            {
                if (InEditMode)
                {
                    BinName = TempBinName;
                    ProjectsPage.SignalProjectsChanged(bin.Project_id);
                }
                else
                {
                    TempBinName = BinName;
                }

                InEditMode = !InEditMode;
            });
            BinEditCancelCommand = ReactiveCommand.Create(() => InEditMode = false);

            Tasks = new ObservableCollection<ProjectTaskViewModel>();
            Tasks.AddRange(bin.Tasks.Select(x => new ProjectTaskViewModel(x)));
            AddTaskCommand = ReactiveCommand.Create(() => Tasks.Add(new ProjectTaskViewModel(new ApiProjectTask() { Name = "New task" }.Populate())));

            Tasks.CollectionChanged += (s, e) => e.FillToList(bin.Tasks, (ProjectTaskViewModel vm) => vm.GetTask());

            this.bin = bin;
        }
    }
}
