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

namespace AvaloniaPlanner.Controls
{
    public class PaneEntryModel : ReactiveObject
    {
        private PaneEntry Parent { get; init; }

        public string EntryName => Parent.EntryName;
        public Material.Icons.MaterialIconKind Icon => Parent.Icon;
        public double IconSize => Parent.IconSize;


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

        public void EntryClicked(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(ClickedEvent));

        public PaneEntry() : this(null) { }

        public PaneEntry(MainViewModel? vm) 
        {
            InitializeComponent();
            this.DataContext = new PaneEntryModel(this);

            var source = vm != null ? vm : MainView.Singleton.ViewModel;
            var cmd = source.PaneOpenedStateChangedCommand as ReactiveCommand<System.Reactive.Unit, bool>;
            if (cmd == null)
                throw new ArgumentNullException("Cannot get pane state changed command or invalid type");

            cmd.Subscribe(x => ViewModel.IsExpanded = x);
        }

        public PaneEntry(string name, Material.Icons.MaterialIconKind icon, Action<PaneEntry>? clickedAction = null, MainViewModel? vm = null) :this(vm)
        {
            this.EntryName = name;
            this.Icon = icon;
            if (clickedAction != null)
                this.Clicked += (s, e) => clickedAction(this);
        }

        public PaneEntry(string name, Material.Icons.MaterialIconKind icon, Type pageType, MainViewModel? vm = null)
            : this(name, icon, (entry) => PageManager.Navigate(pageType), vm) { }
    }
}
