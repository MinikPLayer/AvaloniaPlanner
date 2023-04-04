using Avalonia.Controls;
using AvaloniaPlanner.Controls;
using AvaloniaPlanner.Pages;
using AvaloniaPlanner.Views;
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

        private object currentPage = new ProjectsPage();
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

        private double _iconSize = 25;
        public double IconSize
        {
            get => _iconSize;
            set => this.RaiseAndSetIfChanged(ref _iconSize, value);
        }

        private bool _isSaveAvailable = false;
        public bool IsSaveAvailable
        {
            get => _isSaveAvailable;
            set => this.RaiseAndSetIfChanged(ref _isSaveAvailable, value);
        }

        private bool _canGoBack = false;
        public bool CanGoBack
        {
            get => _canGoBack;
            set => this.RaiseAndSetIfChanged(ref _canGoBack, value);
        }


        public ObservableCollection<PaneEntry> PaneEntries { get; } = new ObservableCollection<PaneEntry>();
        public ICommand PaneOpenedStateChangedCommand { get; init; }
        public ICommand PaneOpenedStateChangedCommand2 { get; init; }
        public ICommand PaneGoBackCommand { get; set; }

        public MainViewModel()
        {
            PaneOpenedStateChangedCommand = ReactiveCommand.Create(() => IsPaneOpened = !IsPaneOpened);
            PaneOpenedStateChangedCommand2 = ReactiveCommand.Create(() => IsPaneOpened = !IsPaneOpened);
            PaneGoBackCommand = ReactiveCommand.Create(PageManager.GoBack);
            //PaneEntries.Add(new PaneEntry("Home", Material.Icons.MaterialIconKind.Home, typeof(HomePage)));
            PaneEntries.Add(new PaneEntry("Projects", Material.Icons.MaterialIconKind.Package, typeof(ProjectsPage)));
        }
    }
}