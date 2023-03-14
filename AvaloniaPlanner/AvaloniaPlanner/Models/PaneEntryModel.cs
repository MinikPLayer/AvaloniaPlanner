using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaPlanner.Models
{
    public class PaneEntryModel
    {
        public string Name { get; set; } = "";
        public Material.Icons.MaterialIconKind Icon { get; set; } = Material.Icons.MaterialIconKind.QuestionMark;
        public ICommand? ClickedCommand { get; set; } = null;

        public PaneEntryModel() { }
        public PaneEntryModel(string name, Material.Icons.MaterialIconKind icon, Action? clickedAction = null)
        {
            this.Name = name;
            this.Icon = icon;
            if (clickedAction != null)
                ClickedCommand = ReactiveCommand.Create(clickedAction);
        }
    }
}
