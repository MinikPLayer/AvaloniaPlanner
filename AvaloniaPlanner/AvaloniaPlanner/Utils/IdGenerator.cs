using AvaloniaPlanner.Pages;
using AvaloniaPlannerLib.Data.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Utils
{
    public static class IdGenerator
    {
        public static ApiProject GenerateID(this ApiProject p)
        {
            do
            {
                p.Id = Guid.NewGuid().ToString();
            } while (ProjectsPage.Projects.Any(x => x.Id == p.Id));

            return p;
        }
    }
}
