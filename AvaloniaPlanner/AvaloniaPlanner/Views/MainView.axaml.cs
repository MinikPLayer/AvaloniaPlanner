using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaPlanner.Controls;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.Pages;
using AvaloniaPlanner.Utils;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Platform;

namespace AvaloniaPlanner.Views
{
    public partial class MainView : UserControl
    {
        public static readonly string DefaultSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AvPlanner", "projects.json");

        private string _currentFilePath = "";
        private static MainView? _singleton = null;

        private Task lastDialogOpenTask = Task.FromResult(true);
        private object dialogTaskMutex = new object();

        public static MainView Singleton
        {
            get
            {
                if (_singleton == null)
                    throw new ArgumentNullException("Singleton is null");

                return _singleton;
            }
            set => _singleton = value;
        }

        public MainViewModel ViewModel 
        { 
            get
            {
                if (DataContext == null || DataContext is not MainViewModel mv)
                    this.DataContext = mv = new MainViewModel();

                return mv;
            } 
        }

        private Task _OpenDialog(object content, Action<object, MessageDialogEventArgs>? handler)
        {
            Task newTask;
            lock(dialogTaskMutex)
            {
                newTask = lastDialogOpenTask.ContinueWith(async t => await Dispatcher.UIThread.InvokeAsync(async () => await MainDialog.ShowDialog(content, handler))).Unwrap();
                lastDialogOpenTask = newTask;
            }
            return newTask;
        }

        public static Task OpenDialog(object content, Action<object, MessageDialogEventArgs>? handler = null) 
            => Singleton._OpenDialog(content, handler);

        public static RuntimePlatformInfo RuntimePlatformInfo { get; } = AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo();
        public void TestPP(object sender, PointerPressedEventArgs e)
        {
            if (RuntimePlatformInfo.IsMobile)
                return;

            var posY = e.GetPosition(this).Y;
            if (MainWindow.Singleton != null && posY < 30 && posY != 0) // In comboBox posY == 0, it won't in MainWindow
            {
                if(e.ClickCount == 2)
                {
                    var state = MainWindow.Singleton.WindowState;
                    if (state == WindowState.Maximized)
                        MainWindow.Singleton.WindowState = WindowState.Normal;
                    else if (state == WindowState.Normal)
                        MainWindow.Singleton.WindowState = WindowState.Maximized;            
                }
                else
                {
                    MainWindow.Singleton.BeginMoveDrag(e);
                }
            }
            
        }

        public void SettingsPageClick(object sender, RoutedEventArgs e) => PageManager.Navigate<SettingsPage>();

        public void SaveProjectClick(object sender, RoutedEventArgs e)
        {
            if (!SaveFile())
            {
                MainView.OpenDialog(new ErrorDialog("Cannot save the output file"));
                return;
            }
            ViewModel.IsSaveAvailable = false;
        }

        public string LastSaveData = "";
        bool LoadFile(string? path = null)
        {
            if (path == null)
                path = DefaultSavePath;

            if (!File.Exists(path))
                return false;

            var data = File.ReadAllText(path);
            if (ProjectsPage.LoadProjectsFromString(data))
            {
                LastSaveData = data;
                _currentFilePath = path;
                return true;
            }

            return false;
        }

        bool SaveFile(string? path = null, bool overwrite = true)
        {
            if (path == null)
                path = _currentFilePath;

            if (!overwrite && File.Exists(path))
                return false;

            var dir = Path.GetDirectoryName(path);
            if (dir == null)
                return false;

            Directory.CreateDirectory(dir);

            var data = ProjectsPage.SerializeProjects();
            LastSaveData = data;
            File.WriteAllText(path, data);

            _currentFilePath = path;
            return true;
        }

        void TestStartup()
        {
            //PageManager.Navigate(new ProjectViewPage(ProjectsPage.Projects[0]));
            //PageManager.Navigate(new ProjectsPage());
        }

        public MainView()
        {
            Singleton = this;
            InitializeComponent();
            NavigationViewSplitView.DataContext = this.DataContext = new MainViewModel();     

            // If file doesn't exists, create one
            SaveFile(path: DefaultSavePath, overwrite: false);
            if(!LoadFile())
            {
                this.Content = new TextBlock
                {
                    Text = "Cannot create or load file from path: " + DefaultSavePath,
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontWeight = FontWeight.ExtraBlack,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, 
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center 
                };
            }
#if DEBUG
            TestStartup();
#endif
        }
    }
}