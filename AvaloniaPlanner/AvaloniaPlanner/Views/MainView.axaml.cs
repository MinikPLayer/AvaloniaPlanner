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
using DynamicData;
using CSUtil.Web;
using AvaloniaPlannerLib.Data.Auth;

namespace AvaloniaPlanner.Views
{
    public partial class MainView : UserControl
    {
        public static readonly string DefaultSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvPlanner", "projects.json");

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

        public static bool IsMobile => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
        public void TestPP(object sender, PointerPressedEventArgs e)
        {
            if (IsMobile)
                return;

            var pos = e.GetPosition(this);
            var vis = Avalonia.VisualTree.VisualExtensions.GetVisualAt(this, pos, x => x is Control c && c.IsHitTestVisible);
            if (sender != vis)
                return;

            var posY = pos.Y;
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
            if (SaveFile() != null)
            {
                MainView.OpenDialog(new ErrorDialog("Cannot save the output file"));
                return;
            }
            ViewModel.IsSaveAvailable = false;
        }

        public string LastSaveData = "";
        public string? LoadFile(string? path = null, bool changeCurrentPath = true)
        {
            try
            {
                if (path == null)
                    path = DefaultSavePath;

                if (!File.Exists(path))
                    return $"File \"{path}\" doesn't exist";

                var data = File.ReadAllText(path);
                var projects = ProjectsPage.LoadProjectsFromString(data);
                if (projects == null) 
                    return "Cannot parse projects from file";
                
                ProjectsPage.Projects.Clear();
                ProjectsPage.Projects.AddRange(projects);

                LastSaveData = data;
                if(changeCurrentPath)
                    _currentFilePath = path;
                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.WriteLine(e);
#if DEBUG
                return e.Message + " in " + e.Source + " : " + e.TargetSite;
#endif
                return e.Message;
            }
        }

        /// <summary>
        /// Saves file to disk
        /// </summary>
        /// <param name="path">Target file path, leave null to use the last used path</param>
        /// <param name="overwrite">True to overwrite already existing files</param>
        /// <param name="changeCurrentPath">Set "last path" to the value of path</param>
        /// <returns>Null if success, error string otherwise</returns>
        public string? SaveFile(string? path = null, bool overwrite = true, bool changeCurrentPath = true)
        {
            try
            {
                if (path == null)
                    path = _currentFilePath;

                if (!overwrite && File.Exists(path))
                    return "File exists";

                var dir = Path.GetDirectoryName(path);
                if (dir == null)
                    return "Directory doesn't exist";

                Directory.CreateDirectory(dir);

                var data = ProjectsPage.SerializeProjects();
                LastSaveData = data;
                File.WriteAllText(path, data);

                if(changeCurrentPath)
                    _currentFilePath = path;
                
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.WriteLine(e);
#if DEBUG
                return e.Message + " in " + e.Source + " : " + e.TargetSite;
#endif
                return e.Message;
            }
        }

        void TestStartup()
        {
            //PageManager.Navigate(new ProjectViewPage(ProjectsPage.Projects[0]));
            //PageManager.Navigate(new ProjectsPage());
        }

        public MainView()
        {
            Singleton = this;
            SettingsPage.LoadConfig();
            SettingsSync.TryLoadSyncToken();
            InitializeComponent();
            
            NavigationViewSplitView.DataContext = this.DataContext = new MainViewModel();     

            // If file doesn't exists, create one
            SaveFile(path: DefaultSavePath, overwrite: false);
            var loadRet = LoadFile();
            if(loadRet != null)
            {
                this.Content = new TextBlock
                {
                    Text = "Cannot create or load file - " + loadRet,
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontWeight = FontWeight.ExtraBlack,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, 
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center 
                };
            }

            Api.OnTokenExpired += RefreshToken;
#if DEBUG
            TestStartup();
#endif
        }

        private async Task<bool> RefreshToken()
        {
            var token = await Api.Post<ApiAuthToken>("api/Auth/refresh_token");
            if (token.IsOk() && token.Payload != null && !string.IsNullOrEmpty(token.Payload.Token))
            {
                SettingsSync.SettingsSyncToken = token.Payload.Token;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}