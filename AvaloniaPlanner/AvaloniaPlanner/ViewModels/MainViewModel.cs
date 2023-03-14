using Avalonia.Controls;
using AvaloniaPlanner.Models;
using AvaloniaPlanner.Pages;
using AvaloniaPlanner.Views;
using DialogHostAvalonia;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public string Greeting => "Welcome to Avalonia!";

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

        public MainViewModel()
        {
            PaneOpenedStateChangedCommand = ReactiveCommand.Create(() => IsPaneOpened = !IsPaneOpened);
            PaneEntries.Add(new PaneEntryModel("Home", Material.Icons.MaterialIconKind.Home, () => Debug.WriteLine("Home!")));
            PaneEntries.Add(new PaneEntryModel("Add", Material.Icons.MaterialIconKind.Add));
            PaneEntries.Add(new PaneEntryModel("Subtract", Material.Icons.MaterialIconKind.Minus));
            PaneEntries.Add(new PaneEntryModel("Multiply", Material.Icons.MaterialIconKind.Asterisk));
        }
    }
}