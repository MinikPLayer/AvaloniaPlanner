using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private string dialogTitle = "";
        public string DialogTitle
        {
            get => dialogTitle;
            set => this.RaiseAndSetIfChanged(ref dialogTitle, value);
        }

        private string dialogMessage = "Test dialog message";
        public string DialogMessage
        {
            get => dialogMessage;
            set => this.RaiseAndSetIfChanged(ref dialogMessage, value);
        }

        private bool dialogOpened = false;
        public bool DialogOpened
        {
            get => dialogOpened;
            set => this.RaiseAndSetIfChanged(ref dialogOpened, value);
        }
    }
}
