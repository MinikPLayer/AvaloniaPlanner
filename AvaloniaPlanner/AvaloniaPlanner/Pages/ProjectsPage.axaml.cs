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
using System;
using DynamicData.Kernel;
using Material.Icons;

namespace AvaloniaPlanner.Pages
{

    public enum ProjectsOrderTypes
    {
        Name,
        Deadline,
        CreationDate,
        LastUpdate
    }
    
    public partial class ProjectsPage : UserControl
    {
        public static ObservableCollection<ApiProject> Projects { get; set; }
        private static List<ProjectsPage> Pages = new List<ProjectsPage>();

        public static void SignalProjectsChanged(string id)
        {
            MainView.Singleton.ViewModel.IsSaveAvailable = true;
            var proj = Projects.FirstOrDefault(x => x.Id == id);
            if(proj != null)
                proj.LastUpdate = DateTime.Now;
        }

        public static string SerializeProjects()
        {
            var ret = JsonConvert.SerializeObject(Projects, Formatting.Indented);
            return ret;
        }

        public static IEnumerable<ApiProject>? LoadProjectsFromString(string data)
        {
            var ret = JsonConvert.DeserializeObject<IEnumerable<ApiProject>>(data);
            if (ret == null)
                return null;

            return ret;
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

        Func<ApiProject, object> ProjectOrderArg = x => x.LastUpdate;
        Func<ApiProject, bool> ProjectPriorityOrderWhereFunc = x => true;

        Func<IEnumerable<ApiProject>, Func<ApiProject, object>, IOrderedEnumerable<ApiProject>> ProjectOrderFunc = (data, orderArg) => data.OrderByDescending(orderArg);
        public void ApplySearchFilter(string? term)
        {
            ProjectsPanel.Children.Clear();
            IEnumerable<ApiProject> projects = Projects;

            if (!string.IsNullOrEmpty(term))
            {
                term = term.ToLower();
                projects = projects.Where(p => p.Name.ToLower().Contains(term));
            }

            var priority = ProjectOrderFunc(projects.Where(ProjectPriorityOrderWhereFunc), ProjectOrderArg);
            var rest = ProjectOrderFunc(projects.Except(priority), ProjectOrderArg);

            ProjectsPanel.Children.AddRange(priority.Concat(rest).Select(p => new ProjectControl(p)));
        }

        private void SetProjectsOrderMethod(ProjectsOrderTypes method)
        {
            ProjectPriorityOrderWhereFunc = x => true;
            switch(method)
            {
                case ProjectsOrderTypes.Deadline:
                    ProjectOrderArg = x => x.Deadline;
                    ProjectPriorityOrderWhereFunc = x => x.IsDeadlineApplicable;
                    break;

                case ProjectsOrderTypes.Name:
                    ProjectOrderArg = x => x.Name;
                    break;

                case ProjectsOrderTypes.LastUpdate:
                    ProjectOrderArg = x => x.LastUpdate;
                    break;

                case ProjectsOrderTypes.CreationDate:
                    ProjectOrderArg = x => x.CreationDate;
                    break;

                default:
                    return;
            }

            SettingsPage.Config.ProjectsOrderType = method;
            if(SearchInputControl != null)
                SearchInputControl.RaiseSearchEvent();
        }

        private void SetProjectsOrderDirection(bool ascending)
        {
            switch (ascending)
            {
                case true:
                    ProjectOrderFunc = (data, orderArg) => data.OrderBy(orderArg);
                    break;

                case false:
                    ProjectOrderFunc = (data, orderArg) => data.OrderByDescending(orderArg);
                    break;
            }

            SettingsPage.Config.ProjectsOrderAscending = ascending;
            if (SearchInputControl != null)
                SearchInputControl.RaiseSearchEvent();
        }

        public static void RemoveProject(ApiProject project)
        {
            MainView.OpenDialog(new ConfirmDialog("Are you sure you want to delete project " + project.Name + "?"), (s, e) =>
            {
                var result = e.Parameter;
                if (result is bool b && b == true)
                {
                    var ret = Projects.Remove(project);
                    SignalProjectsChanged(project.Id);
                }
            });
        }

        public static void EditProject(ApiProject project)
        {
            MainView.OpenDialog(new ProjectEditDialog(project), (s, e) =>
            {
                var result = e.Parameter;
                if (result is bool b && b == true && e.Content is ProjectEditDialog dialog)
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
            });
        }

        public static ApiProject? GetProject(string id) => Projects.FirstOrDefault(x => x.Id == id);

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
            ApplySearchFilter(null);
            
            OrderSelector.SetOrderMethods(new List<OrderSelection>() {
                new OrderSelection("Name", MaterialIconKind.SortAlphabetically, ProjectsOrderTypes.Name),
                new OrderSelection("Deadline", MaterialIconKind.CalendarClock, ProjectsOrderTypes.Deadline),
                new OrderSelection("Last update", MaterialIconKind.Update, ProjectsOrderTypes.LastUpdate),
                new OrderSelection("Creation date", MaterialIconKind.Calendar, ProjectsOrderTypes.CreationDate),
            }, SettingsPage.Config.ProjectsOrderType, SettingsPage.Config.ProjectsOrderAscending);
        }

        ~ProjectsPage()
        {
            Pages.Remove(this);
        }

        private void OrderSelector_OnOrderMethodChanged(object? sender, OrderSelectorEventArgs e)
        {
            if(e.Method is not ProjectsOrderTypes type)
                return;
            
            SetProjectsOrderMethod(type);
            SetProjectsOrderDirection(e.Ascending);
        }
    }
}
