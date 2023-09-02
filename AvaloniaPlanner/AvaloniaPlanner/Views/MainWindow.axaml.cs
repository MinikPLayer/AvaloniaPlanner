using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using System;

namespace AvaloniaPlanner.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Singleton = null;

        public MainWindow()
        {
            Singleton = this;
            this.DataContext = new MainWindowViewModel();
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }
    }
}