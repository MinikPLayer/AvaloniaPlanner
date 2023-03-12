using Avalonia.Controls;
using AvaloniaPlanner.Models;
using AvaloniaPlanner.Pages;
using AvaloniaPlanner.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

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

        private object currentPage = new HomePage();
        public object CurrentPage
        {
            get => currentPage;
            set => this.RaiseAndSetIfChanged(ref currentPage, value);
        }

        private bool isPaneOpened = false;
        public bool IsPaneOpened
        {
            get => isPaneOpened;
            set => this.RaiseAndSetIfChanged(ref isPaneOpened, value);
        }

        public ObservableCollection<PaneEntryModel> PaneEntries { get; } = new ObservableCollection<PaneEntryModel>();

        public ICommand PaneOpenedStateChangedCommand { get; init; }

        public string Greeting => "Welcome to Avalonia!";

        public MainViewModel()
        {
            PaneOpenedStateChangedCommand = ReactiveCommand.Create(() => IsPaneOpened = !IsPaneOpened);

            PaneEntries.Add(new PaneEntryModel("test name", Material.Icons.MaterialIconKind.Add));
        }
	}
}