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
using AvaloniaPlanner.Dialogs;
using DialogHostAvalonia;
using System;

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
                    Pages.ForEach(p => p.SearchInputControl.ResetSearch(null, null));

                Pages.ForEach(p => p.SearchInputControl.RaiseSearchEvent());

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

        private static bool dialogOpened = false;
        public static void RemoveProject(ApiProject project)
        {
            if (dialogOpened)
                return;

            dialogOpened = true;
            DialogHost.Show(new ConfirmDialog("Are you sure you want to delete project " + project.Name + "?"), closingEventHandler: (s, e) =>
            {
                var result = e.Parameter;
                if (result is bool b && b == true)
                {
                    var ret = Projects.Remove(project);
                    SignalProjectsChanged(project.Id);
                }

                dialogOpened = false;
            });
        }

        public static void EditProject(ApiProject project)
        {
            if (dialogOpened)
                return;

            dialogOpened = true;
            DialogHost.Show(new ProjectEditDialog(project), closingEventHandler: (s, e) =>
            {
                var result = e.Parameter;
                if (result is bool b && b == true && e.Session.Content is ProjectEditDialog dialog)
                {
                    if (dialog.DataContext is not ProjectViewModel newProjectVm)
                        throw new Exception("Dialog data context is an invalid type");

                    // ObservableCollection.Replace() is not working properly (entry disappears only to reappear after next refresh)
                    // So we need to use .Replace() twice to refresh after disappearing
                    var newProject = newProjectVm.GetProject();
                    Projects.Replace(project, newProject);
                    Projects.Replace(newProject, newProject);
                    SignalProjectsChanged(newProject.Id);
                }

                dialogOpened = false;
            });
        }

        public void AddProjectButtonClicked(object sender, RoutedEventArgs e)
        {
            var project = new ApiProject() { Name = "New project", Description = "New project description" }.Populate();
            Projects.Add(project);
            SignalProjectsChanged(project.Id);

            EditProject(project);
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
