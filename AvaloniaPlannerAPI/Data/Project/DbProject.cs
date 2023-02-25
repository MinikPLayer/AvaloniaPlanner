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
        [SQLSize(36)]
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Owner { get; set; } = "";

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; } = DateTime.Now;

        public DbProject() { }

        public DbProject(StringID id, string name, string description, StringID owner)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Owner = owner;

            this.CreationDate = DateTime.Now;
            this.LastUpdate = this.CreationDate;
        }

        public DbProject(Database db, string name, string description, StringID owner)
            : this(db.GenerateUniqueIdString(TABLE_NAME, nameof(DbProject.Id)), name, description, owner) { }
    }
}
