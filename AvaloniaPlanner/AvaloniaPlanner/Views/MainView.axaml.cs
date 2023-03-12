using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib;
using AvAPI.Model;
using DialogHostAvalonia;
using Newtonsoft.Json;
using System.IO;
using System.Xml;

namespace AvaloniaPlanner.Views
{
    public partial class MainView : UserControl
    {
        void LoadFile(string path)
        {
            //var data = File.ReadAllText(path);
            //var apiProject = JsonConvert.DeserializeObject<ApiProject>(data);
        }

        public async void TestClick(object sender, RoutedEventArgs args)
        {
            var cStackPanel = new StackPanel();
            var tb = new TextBlock() { Text = "Test from MainView" };
            var btn = new Button() { Content = "Ok" };
            btn.Click += (a, s) => TestDialog.CloseDialogCommand.Execute(null);
            cStackPanel.Children.Add(tb);
            cStackPanel.Children.Add(btn);

            await DialogHost.Show(cStackPanel, "TestDialog");
            TestDialog.IsOpen = true;
        }

        public MainView()
        {
            InitializeComponent();

            LoadFile("C:\\Users\\Minik\\Documents\\Projekty\\AvProject.json");
        }
    }
}