using AvaloniaPlannerLib.Data.Project;
using CSUtil.Data;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public OList<ProjectTaskViewModel> Tasks { get; set; }

        public ProjectBinViewModel(ApiProjectBin bin)
        {
            Tasks = new OList<ProjectTaskViewModel>();
            Tasks.AddRange(bin.Tasks.Select(x => new ProjectTaskViewModel(x)));
            Tasks.OnCollectionChanged += (c) => this.RaisePropertyChanged(nameof(Tasks));

            this.bin = bin;
        }
    }
}
