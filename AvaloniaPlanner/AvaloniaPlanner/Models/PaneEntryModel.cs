using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Models
{
    public class PaneEntryModel
    {
        public string Name { get; set; } = "";
        public Material.Icons.MaterialIconKind Icon { get; set; } = Material.Icons.MaterialIconKind.QuestionMark;

        public PaneEntryModel() { }
        public PaneEntryModel(string name, Material.Icons.MaterialIconKind icon)
        {
            this.Name = name;
            this.Icon = icon;
        }
    }
}
