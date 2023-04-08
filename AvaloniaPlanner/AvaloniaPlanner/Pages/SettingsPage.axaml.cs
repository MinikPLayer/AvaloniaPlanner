using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.Utils;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlanner.Views;
using CSUtil.Web;
using System;
using System.Diagnostics;
using System.IO;

namespace AvaloniaPlanner.Pages
{
    public partial class SettingsPage : UserControl
    {
        public class ConfigData
        {
            public struct ServerData
            {
                public string IP { get; set; }
                public short Port { get; set; }

                public ServerData()
                {
                    IP = "mtomecki.pl";
                    Port = 25110;
                }
            }

            private ServerData _server = new ServerData();
            public ServerData Server
            {
                get => _server;
                set
                {
                    _server = value;
                    Api.baseUrl = _server.IP;
                    Api.port = _server.Port;
                }
            }

            private ProjectsOrderTypes _projectsOrderType = ProjectsOrderTypes.Deadline;

            public ProjectsOrderTypes ProjectsOrderType
            {
                get => _projectsOrderType;
                set
                {
                    _projectsOrderType = value;
                    SettingsPage.SaveConfig();
                }
            }

            private bool _projectsOrderAscending = false;

            public bool ProjectsOrderAscending
            {
                get => _projectsOrderAscending;
                set 
                {
                    _projectsOrderAscending = value;
                    SettingsPage.SaveConfig();
                }
            }

            private TaskOrderingModes _tasksOrderMode = TaskOrderingModes.Priority;

            public TaskOrderingModes TasksOrderMode
            {
                get => _tasksOrderMode;
                set
                {
                    _tasksOrderMode = value;
                    SettingsPage.SaveConfig();
                }
            }

            private bool _tasksOrderAscending = false;
            public bool TasksOrderAscending
            {
                get => _tasksOrderAscending;
                set
                {
                    _tasksOrderAscending = value;
                    SettingsPage.SaveConfig();
                }
            }

            public ConfigData()
            {
                Server = new ServerData();
            }
        }

        static string DefaultConfigSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvPlanner", "config.json");
        public static ConfigData Config { get; set; } = new ConfigData();

        public static void LoadConfig()
        {
            var config = DeserializeConfigFromFile();
            if (config != null)
                Config = config;
        }

        public static ConfigData? DeserializeConfigFromFile(string? filePath = null)
        {
            if (filePath == null)
                filePath = DefaultConfigSavePath;

            if (!File.Exists(filePath))
                return null;
            try
            {
                var data = File.ReadAllText(filePath);
                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigData>(data);
                return config;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot load config from file - " + ex.Message);
                return null;
            }
        }

        public static bool SaveConfig(string? filePath = null)
        {
            if (filePath == null)
                filePath = DefaultConfigSavePath;
            
            var path = Path.GetDirectoryName(filePath);
            if (path == null)
                throw new Exception("Invalid path, cannot extract a directory - " + filePath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            try
            {
                File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(Config));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot save config to file - " + ex.Message);
                return false;
            }
        }

        public async void SaveServerInfoClicked(object sender, RoutedEventArgs e)
        {
            var serverData = new ConfigData.ServerData();
            if(short.TryParse(ServerPortTextBox.Text, out short ret))
            {
                serverData.Port = ret;
            }
            else
            {
                await MainView.OpenDialog(new ErrorDialog("Invalid port, cannot convert to integer"));
                return;
            }
            serverData.IP = ServerIpTextBox.Text!;

            Config.Server = serverData;
            ((SettingsViewModel)this.DataContext!).IsLocked = true;
            var con = await SettingsSync.TestConnection();
            if(!con)
            {
                await MainView.OpenDialog(new ErrorDialog("Cannot connect to the server"));
                ((SettingsViewModel)this.DataContext).ConnectionStatus = false.ToString();
            }
            else
            {
                _ = MainView.OpenDialog(new ErrorDialog("Connected to the server", Material.Icons.MaterialIconKind.Check, Colors.LightGreen));
                ((SettingsViewModel)this.DataContext).ConnectionStatus = true.ToString();
                SaveConfig();
            }

            ((SettingsViewModel)this.DataContext).IsLocked = false;
        }

        public SettingsPage()
        {
            InitializeComponent();
            this.DataContext = new SettingsViewModel();

            ServerPortTextBox.Text = Config.Server.Port.ToString();
            ServerIpTextBox.Text = Config.Server.IP;
        }
    }
}
