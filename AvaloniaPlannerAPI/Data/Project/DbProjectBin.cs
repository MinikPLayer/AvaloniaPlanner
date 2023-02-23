﻿using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    [SqlTable(TABLE_NAME)]
    public class DbProjectBin
    {
        public const string TABLE_NAME = "project_bins";

        [SQLPrimary]
        public long Id { get; set; } = -1;
        public long Project_id { get; set; } = -1;
        public string Name { get; set; } = "";
    }
}
