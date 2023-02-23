﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    public class ApiProjectTask
    {
        public long Id { get; set; } = -1;
        public long Project_id { get; set; } = -1;
        public long Bin_id { get; set; } = -1;
        public string Name { get; set; } = "";
        public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;
    }
}
