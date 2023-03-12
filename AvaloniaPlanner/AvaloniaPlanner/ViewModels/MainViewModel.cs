using Avalonia.Controls;
using AvaloniaPlanner.Views;
using ReactiveUI;

namespace AvaloniaPlanner.ViewModels
{
    public class MainViewModel : ReactiveObject
    {

        private string dialogMessage = "Test dialog message";
        public string DialogMessage
        {
            get => dialogMessage;
            set => this.RaiseAndSetIfChanged(ref dialogMessage, value);
        }

        public object ContentTest { get; set; } = new TestView();

        public string Greeting => "Welcome to Avalonia!";
	}
}