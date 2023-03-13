using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib;
using AvAPI.Model;
using DialogHostAvalonia;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml;

namespace AvaloniaPlanner.Views
{
    public partial class MainView : UserControl
    {
        public MainViewModel ViewModel 
        { 
            get
            {
                if (DataContext == null || DataContext is not MainViewModel mv)
                    throw new Exception("Data context is null or invalid type");

                return mv;
            } 
        }

        public void TestPP(object sender, PointerPressedEventArgs e)
        {
            var posY = e.GetPosition(this).Y;
            if (MainWindow.Singleton != null && posY < 30)
                MainWindow.Singleton.BeginMoveDrag(e);
            
        }

        void LoadFile(string path)
        {
            //var data = File.ReadAllText(path);
            //var apiProject = JsonConvert.DeserializeObject<ApiProject>(data);
        }

        public MainView()
        {
            InitializeComponent();

            LoadFile("C:\\Users\\Minik\\Documents\\Projekty\\AvProject.json");
        }
    }
}