using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;
using System.Windows.Input;
using System;
using AvaloniaPlanner.Pages;
using Avalonia;
using AvaloniaPlanner.Views;
using Avalonia.Data;
using AvaloniaPlanner.ViewModels;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data.Common;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;
using Avalonia.Media;

namespace AvaloniaPlanner.Controls
{
    public class PaneEntryModel : ReactiveObject
    {
        private PaneEntry Parent { get; init; }

        public string EntryName => Parent.EntryName;
        public Material.Icons.MaterialIconKind Icon => Parent.Icon;
        public double IconSize => Parent.IconSize;

        private bool isEnabled = true;
        public bool Enabled
        {
            get => isEnabled;
            set => this.RaiseAndSetIfChanged(ref isEnabled, value);
        }


        private Brush iconBrush = new SolidColorBrush(Colors.White);
        public Brush IconBrush
        {
            get => iconBrush;
            set => this.RaiseAndSetIfChanged(ref iconBrush, value);
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public PaneEntryModel(PaneEntry parent)
        {
            this.Parent = parent;
        }
    }

    public partial class PaneEntry : UserControl, INotifyPropertyChanged
    {
        public PaneEntryModel ViewModel
        {
            get
            {
                if (this.DataContext == null || this.DataContext is not PaneEntryModel vm)
                    throw new NullReferenceException("PaneEntry Data context is null or invalid type");

                return vm;
            }
        }

        public static readonly StyledProperty<double> IconSizeProperty =
            AvaloniaProperty.Register<PaneEntry, double>(nameof(IconSize), 25);

        public double IconSize
        {
            get => GetValue(IconSizeProperty);
            set
            {
                SetValue(IconSizeProperty, value);
                ViewModel.RaisePropertyChanged(nameof(ViewModel.IconSize));
            }
        }

        public static readonly StyledProperty<Material.Icons.MaterialIconKind> IconProperty =
            AvaloniaProperty.Register<PaneEntry, Material.Icons.MaterialIconKind>(nameof(Icon), Material.Icons.MaterialIconKind.QuestionMark);

        public Material.Icons.MaterialIconKind Icon
        {
            get => GetValue(IconProperty);
            set
            {
                SetValue(IconProperty, value);
                ViewModel.RaisePropertyChanged(nameof(ViewModel.Icon));
            }
        }

        public static readonly StyledProperty<string> EntryNameProperty =
            AvaloniaProperty.Register<PaneEntry, string>(nameof(Icon));

        public string EntryName
        {
            get => GetValue(EntryNameProperty);
            set
            {
                SetValue(EntryNameProperty, value);
                ViewModel.RaisePropertyChanged(nameof(ViewModel.EntryName));
            }
        }

        public static RoutedEvent<RoutedEventArgs> ClickedEvent =
            RoutedEvent.Register<PaneEntry, RoutedEventArgs>(nameof(Clicked), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> Clicked
        {
            add => AddHandler(ClickedEvent, value);
            remove => RemoveHandler(ClickedEvent, value);
        }

        public void EntryClicked(object sender, RoutedEventArgs e)
        {
            MainView.Singleton.ViewModel.IsPaneOpened = false;
            RaiseEvent(new RoutedEventArgs(ClickedEvent));
        }

        public PaneEntry() 
        {
            InitializeComponent();
            this.DataContext = new PaneEntryModel(this);
        }

        public PaneEntry(string name, Material.Icons.MaterialIconKind icon, Action<PaneEntry>? clickedAction = null) :this()
        {
            this.EntryName = name;
            this.Icon = icon;
            if (clickedAction != null)
                this.Clicked += (s, e) => clickedAction(this);
        }

        public PaneEntry(string name, Material.Icons.MaterialIconKind icon, Type pageType)
            : this(name, icon, (entry) => PageManager.Navigate(pageType)) { }
    }
}
