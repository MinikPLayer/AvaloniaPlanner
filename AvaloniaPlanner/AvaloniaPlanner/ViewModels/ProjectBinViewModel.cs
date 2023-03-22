using AvaloniaPlanner.Pages;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Data;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaPlanner.ViewModels
{
    public class ProjectBinViewModel : ReactiveObject
    {
        private ApiProjectBin bin;

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

        public ICommand BinEditCommand { get; set; }
        public ICommand BinEditCancelCommand { get; set; }

        public OList<ProjectTaskViewModel> Tasks { get; set; }

        public ProjectBinViewModel(ApiProjectBin bin)
        {
            BinEditCommand = ReactiveCommand.Create(() =>
            {
                if (InEditMode)
                {
                    BinName = TempBinName;
                    ProjectsPage.SignalProjectsChanged();
                }
                else
                {
                    TempBinName = BinName;
                }

                InEditMode = !InEditMode;
            });

            BinEditCancelCommand = ReactiveCommand.Create(() => InEditMode = false);

            Tasks = new OList<ProjectTaskViewModel>();
            Tasks.AddRange(bin.Tasks.Select(x => new ProjectTaskViewModel(x)));
            Tasks.OnCollectionChanged += (c) => this.RaisePropertyChanged(nameof(Tasks));

            this.bin = bin;
        }
    }
}
