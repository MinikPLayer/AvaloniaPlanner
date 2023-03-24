using AvaloniaPlanner.Pages;
using AvaloniaPlannerLib.Data.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Utils
{
    public static class ClassPopulator
    {
        public static ApiProject Populate(this ApiProject p)
        {
            do
            {
                p.Id = Guid.NewGuid().ToString();
            } while (ProjectsPage.Projects.Any(x => x.Id == p.Id));

            p.CreationDate = DateTime.Now;
            p.LastUpdate = DateTime.Now;
            return p;
        }

        public static ApiProjectBin Populate(this ApiProjectBin b)
        {
            do
            {
                b.Id = Guid.NewGuid().ToString();
            } while (ProjectsPage.Projects.SelectMany(x => x.Bins).Any(x => x.Id == b.Id));

            return b;
        }
    }
}
