using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    public class ApiProject
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Owner { get; set; } = "";

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }
}
