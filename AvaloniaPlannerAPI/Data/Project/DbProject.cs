using CSUtil.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlannerLib.Data.Project
{
    [SqlTable(TABLE_NAME)]
    public class DbProject
    {
        public const string TABLE_NAME = "projects";

        [SQLPrimary]
        public long Id { get; set; } = -1;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public long Owner { get; set; } = -1;

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; } = DateTime.Now;

        public DbProject() { }

        public DbProject(long id, string name, string description, long author)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Owner = author;

            this.CreationDate = DateTime.Now;
            this.LastUpdate = this.CreationDate;
        }
    }
}
