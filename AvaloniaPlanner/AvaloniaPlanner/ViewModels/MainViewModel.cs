﻿using Avalonia.Controls;
using AvaloniaPlanner.Controls;
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

        private double _iconSize = 15;
        public double IconSize
        {
            get => _iconSize;
            set => this.RaiseAndSetIfChanged(ref _iconSize, value);
        }


        public ObservableCollection<PaneEntry> PaneEntries { get; } = new ObservableCollection<PaneEntry>();
        public ICommand PaneOpenedStateChangedCommand { get; init; }
        public ICommand PaneOpenedStateChangedCommand2 { get; init; }

        public MainViewModel()
        {
            PaneOpenedStateChangedCommand = ReactiveCommand.Create(() => IsPaneOpened = !IsPaneOpened);
            PaneOpenedStateChangedCommand2 = ReactiveCommand.Create(() => IsPaneOpened = !IsPaneOpened);
            PaneEntries.Add(new PaneEntry("Home", Material.Icons.MaterialIconKind.Home, typeof(HomePage), this));
            PaneEntries.Add(new PaneEntry("Settings", Material.Icons.MaterialIconKind.Settings, typeof(SettingsPage), this));
        }
    }
}