using Avalonia.Controls;
using AvaloniaPlanner.ViewModels;
using CSUtil.Data;
using ReactiveUI;
using System.Collections.Generic;
using AvaloniaPlanner.Controls;
using AvaloniaPlannerLib.Data.Project;
using System.Linq;

namespace AvaloniaPlanner.Pages
{

    public partial class ProjectsPage : UserControl
    {
        public static OList<ApiProject> Projects { get; set; }
        private static List<ProjectsPage> Pages = new List<ProjectsPage>();

        public static void SignalProjectsChanged() => Projects.OnCollectionChanged?.Invoke(Projects);

        static ProjectsPage()
        {
            Projects = new OList<ApiProject>
            {
                // Test project
                new ApiProject { Name = "Test project", Owner = "Test author", Description = "This is a test project, nothing less, nothing more", Bins = new List<ApiProjectBin>() { new ApiProjectBin { Name = "To-Do", Tasks = new List<ApiProjectTask>() { new ApiProjectTask() { Name = "Task 1", status = ProjectStatus.Supported }, new ApiProjectTask() { Name = "Task 2", status = ProjectStatus.Defined } } }, new ApiProjectBin { Name = "In Progress", Tasks = new List<ApiProjectTask>() { new ApiProjectTask() { Name = "Task 3", status = ProjectStatus.InProgress } } } } },
                new ApiProject { Name = "Test project 2", Owner = "Test author", Description = "This is also a test project, but does it matter? I don't really think so. It's just data, the same type of data. That's what matters", Bins = new List<ApiProjectBin>() { new ApiProjectBin { Name = "In Progress" }, new ApiProjectBin { Name = "Done" }, new ApiProjectBin { Name = "Archived" } } }
            };

            Projects.OnItemAdded += (list, ind, item) => Pages.ForEach(p => p.ProjectsPanel.Children.Add(new ProjectControl(item)));
            Projects.OnItemRemoved += (list, ind, item) =>
            {
                foreach(var page in Pages)
                {
                    var toRemove = page.ProjectsPanel.Children.OfType<ProjectControl>().Where(x => x.IsProject(item));
                    page.ProjectsPanel.Children.RemoveAll(toRemove);
                }
            };
        }

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
