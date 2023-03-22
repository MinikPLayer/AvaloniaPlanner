using Avalonia.Controls;
using AvaloniaPlanner.ViewModels;
using CSUtil.Data;
using ReactiveUI;
using System.Collections.Generic;
using AvaloniaPlanner.Controls;
using AvaloniaPlannerLib.Data.Project;
using System.Linq;
using AvaloniaPlanner.Views;
using Newtonsoft.Json;
using DynamicData;
using Avalonia.Interactivity;
using AvaloniaPlanner.Utils;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AvaloniaPlanner.Pages
{

    public partial class ProjectsPage : UserControl
    {
        public static ObservableCollection<ApiProject> Projects { get; set; }
        private static List<ProjectsPage> Pages = new List<ProjectsPage>();

        public static void SignalProjectsChanged(string id)
        {
            MainView.Singleton.ViewModel.IsSaveAvailable = true;
            Debug.WriteLine("Updated project " + id);
        }

        public static string SerializeProjects()
        {
            var ret = JsonConvert.SerializeObject(Projects, Formatting.Indented);
            return ret;
        }

        public static bool LoadProjectsFromString(string data)
        {
            var ret = JsonConvert.DeserializeObject<IEnumerable<ApiProject>>(data);
            if (ret == null)
                return false;

            Projects.Clear();
            Projects.AddRange(ret);
            return true;
        }

        static ProjectsPage()
        {
            Projects = new ObservableCollection<ApiProject>();
            Projects.CollectionChanged += (sender, e) =>
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                    Pages.ForEach(p => p.ApplySearchFilter(""));
            };
        }

        public void ApplySearchFilter(string? term)
        {
            ProjectsPanel.Children.Clear();
            IEnumerable<ApiProject> projects = Projects;
            if(!string.IsNullOrEmpty(term))
                projects = projects.Where(p => p.Name.Contains(term));

            ProjectsPanel.Children.AddRange(projects.Select(p => new ProjectControl(p)));
        }

        public void AddProjectButtonClicked(object sender, RoutedEventArgs e)
        {
            var project = new ApiProject().GenerateID();
            Projects.Add(project);
            SignalProjectsChanged(project.Id);
        }

        public void ProjectSearchRequested(object sender, SearchEventArgs e) => ApplySearchFilter(e.SearchTerm);

        public ProjectsPage()
        {
            InitializeComponent();
            Pages.Add(this);
            this.ProjectsPanel.Children.AddRange(Projects.Select(x => new ProjectControl(x)));
        }

        ~ProjectsPage()
        {
            Pages.Remove(this);
        }
    }
}
