using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.Pages;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib;
using DialogHostAvalonia;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace AvaloniaPlanner.Views
{
    public partial class MainView : UserControl
    {
        public static readonly string DefaultSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AvPlanner", "projects.json");

        private string _currentFilePath = "";
        private static MainView? _singleton = null;
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

        public void TestPP(object sender, PointerPressedEventArgs e)
        {
            var posY = e.GetPosition(this).Y;
            if (MainWindow.Singleton != null && posY < 30)
                MainWindow.Singleton.BeginMoveDrag(e);
            
        }

        public void TestClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Test click");
        }

        public void SaveProjectClick(object sender, RoutedEventArgs e)
        {
            if(!SaveFile())
            {
                DialogHost.Show(new ErrorDialog("Cannot save the output file"));
                return;
            }
            ViewModel.IsSaveAvailable = false;
        }

        bool LoadFile(string? path = null)
        {
            if (path == null)
                path = DefaultSavePath;

            if (!File.Exists(path))
                return false;

            var data = File.ReadAllText(path);
            if(ProjectsPage.LoadProjectsFromString(data))
            {
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
            File.WriteAllText(path, data);

            _currentFilePath = path;
            return true;
        }

        void TestStartup()
        {
            PageManager.Navigate<ProjectsPage>();
        }

        public MainView()
        {
            Singleton = this;
            this.DataContext = new MainViewModel();
            InitializeComponent();

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