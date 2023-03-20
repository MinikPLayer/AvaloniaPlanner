﻿using AvaloniaPlannerLib.Data.Project;
using Material.Icons;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Models
{
    public class ProjectStatusModel : ReactiveObject
    {
        public static List<ProjectStatusModel> Statuses { get; set; } = new List<ProjectStatusModel>()
        {
            new ProjectStatusModel(ProjectStatus.Abandoned, MaterialIconKind.Cancel),
            new ProjectStatusModel(ProjectStatus.Archived, MaterialIconKind.Archive),
            new ProjectStatusModel(ProjectStatus.Completed, MaterialIconKind.Done),
            new ProjectStatusModel(ProjectStatus.Defined, MaterialIconKind.Application),
            new ProjectStatusModel(ProjectStatus.Idea, MaterialIconKind.LightbulbOn),
            new ProjectStatusModel(ProjectStatus.InProgress, MaterialIconKind.AccountHardHat),
            new ProjectStatusModel(ProjectStatus.Prototype, MaterialIconKind.TestTube),
            new ProjectStatusModel(ProjectStatus.Supported, MaterialIconKind.FaceAgent),
            new ProjectStatusModel(ProjectStatus.Unknown, MaterialIconKind.QuestionMark),
        };

        private ProjectStatus _status = ProjectStatus.Unknown;
        public ProjectStatus Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        private MaterialIconKind _icon = MaterialIconKind.QuestionMark;
        public MaterialIconKind Icon
        {
            get => _icon;
            set => this.RaiseAndSetIfChanged(ref _icon, value);
        }

        public static ProjectStatusModel Get(ProjectStatus status)
        {
            var ret = Statuses.Find(x => x.Status == status);
            if (ret != null)
                return ret;

            return new ProjectStatusModel(status, MaterialIconKind.QuestionMark);
        }

        private ProjectStatusModel(ProjectStatus status, MaterialIconKind icon)
        {
            this.Status = status;
            this.Icon = icon;
        }
    }
}
