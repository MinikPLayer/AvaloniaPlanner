using Avalonia.Controls;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlannerLib;
using DialogHostAvalonia;
using System.Xml;

namespace AvaloniaPlanner.Views
{
    public partial class MainView : UserControl
    {
        void LoadFile(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);
        }

        public MainView()
        {
            InitializeComponent();

            LoadFile("C:\\Users\\Minik\\Documents\\Projekty\\AvaloniaPlanner.tpf");
        }
    }
}