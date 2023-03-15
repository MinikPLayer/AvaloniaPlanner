using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib;
using AvAPI.Model;
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

        void LoadFile(string path)
        {
            //var data = File.ReadAllText(path);
            //var apiProject = JsonConvert.DeserializeObject<ApiProject>(data);
        }

        public MainView()
        {
            Singleton = this;
            this.DataContext = new MainViewModel();
            InitializeComponent();

            LoadFile("C:\\Users\\Minik\\Documents\\Projekty\\AvProject.json");
        }
    }
}