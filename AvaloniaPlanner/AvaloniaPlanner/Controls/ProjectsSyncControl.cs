using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.Pages;
using AvaloniaPlanner.Views;
using CSUtil.Web;
using DynamicData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Controls
{
    public class ProjectsSyncControl : PaneEntry
    {
        const string DEFAULT_NAME = "Projects sync";
        const string SUCCESS_NAME = "Projects synced";

        const Material.Icons.MaterialIconKind DEFAULT_ICON = Material.Icons.MaterialIconKind.Sync;
        const Material.Icons.MaterialIconKind SUCCESS_ICON = Material.Icons.MaterialIconKind.Check;
        const Material.Icons.MaterialIconKind FAILURE_ICON = Material.Icons.MaterialIconKind.SyncAlert;

        static readonly SolidColorBrush DEFAULT_BRUSH = new SolidColorBrush(Colors.White);
        static readonly SolidColorBrush WORKING_BRUSH = new SolidColorBrush(Colors.Orange);
        static readonly SolidColorBrush SUCCESS_BRUSH = new SolidColorBrush(Colors.Green);
        static readonly SolidColorBrush FAILURE_BRUSH = new SolidColorBrush(Colors.Red);

        public ProjectsSyncControl()
        {
            this.Icon = DEFAULT_ICON;
            this.EntryName = DEFAULT_NAME;
            this.Clicked += ProjectsSyncControl_Clicked;
            this.ViewModel.IconBrush = DEFAULT_BRUSH;

            InitializeComponent();
        }

        async Task<string?> DownloadData()
        {
            var projectsData = await Api.Get<string>("api/project/get_user_projects");
            if (!projectsData)
                return projectsData.Message;

            var projects = ProjectsPage.LoadProjectsFromString(projectsData.Payload!);
            if (projects == null)
                return "Cannot parse data from the server";

            ProjectsPage.Projects.Clear();
            ProjectsPage.Projects.AddRange(projects);

            MainView.Singleton.SaveFile();
            return null;
        }

        private async Task ResetStatusTask(int curSyncCounter)
        {
            await Task.Delay(3000);
            if (curSyncCounter == syncCounter)
            {
                this.Icon = DEFAULT_ICON;
                this.ViewModel.IconBrush = DEFAULT_BRUSH;
                this.EntryName = DEFAULT_NAME;
            }
        }

        bool syncInProgress = false;
        int syncCounter = 0;
        private async void ProjectsSyncControl_Clicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (syncInProgress)
                return;

            syncInProgress = true;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                this.Icon = DEFAULT_ICON;
                this.ViewModel.IconBrush = WORKING_BRUSH;
                this.EntryName = "Syncing...";

                // Add a small minimum sync duration time to show UI changes for the user
                var delayTask = Task.Delay(150);

                var data = MainView.Singleton.LastSaveData;
                var projects = ProjectsPage.LoadProjectsFromString(data);
                var lastUpdate = DateTime.MinValue;
                if(projects != null)
                    lastUpdate = projects.Select(x => x.LastUpdate).OrderByDescending(x => x).FirstOrDefault();              

                var isWaiting = true;
                _ = Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    this.EntryIcon.RenderTransform = new RotateTransform(0);
                    while (isWaiting)
                    {
                        var rot = (RotateTransform)this.EntryIcon.RenderTransform;
                        rot.Angle -= 10;
                        this.EntryIcon.InvalidateVisual();
                        await Task.Delay(30);
                    }
                    this.EntryIcon.RenderTransform = new RotateTransform(0);
                });

                var serverLastUpdate = await Api.Get<DateTime>("api/project/get_last_modification_date");
                if(!serverLastUpdate)
                {
                    isWaiting = false;
                    syncInProgress = false;
                    this.Icon = FAILURE_ICON;
                    this.ViewModel.IconBrush = FAILURE_BRUSH;
                    this.EntryName = serverLastUpdate.Message!;
                    return;
                }

                bool continueUpload = true;
                if(serverLastUpdate.Payload > lastUpdate)
                {
                    SyncDialogResults result = SyncDialogResults.Cancel;
                    // No local data
                    if (lastUpdate == DateTime.MinValue)
                        result = SyncDialogResults.Download;
                    else
                        result = await SyncConflictDialog.ShowDialog($"Server conflict. \nServer last update: {serverLastUpdate.Payload}\nLocal last update: {lastUpdate}");

                    if (result != SyncDialogResults.ForceUpload)
                    {
                        continueUpload = false;
                        string? msg = null;
                        if (result == SyncDialogResults.Download)
                            msg = await DownloadData();
                        
                        if(msg != null)
                        {
                            this.Icon = FAILURE_ICON;
                            this.ViewModel.IconBrush = FAILURE_BRUSH;
                            this.EntryName = msg;
                        }
                        else
                        {
                            this.Icon = SUCCESS_ICON;
                            this.ViewModel.IconBrush = SUCCESS_BRUSH;
                            this.EntryName = "Download complete";
                        }
                    }
                }

                ApiResult<object>? uploadResult = null;
                if(continueUpload)
                {
                    uploadResult = await Api.Post<object>("api/project/update_user_projects", "data".ToApiParam(data));
                }
                isWaiting = false;

                await delayTask;
                syncCounter++;
                var curSyncCounter = syncCounter;
                syncInProgress = false;

                if (continueUpload && !uploadResult!)
                {
                    this.Icon = FAILURE_ICON;
                    this.ViewModel.IconBrush = FAILURE_BRUSH;
                    this.EntryName = uploadResult!.Message!;
                    return;
                }
                else if(continueUpload)
                {
                    this.Icon = SUCCESS_ICON;
                    this.ViewModel.IconBrush = SUCCESS_BRUSH;
                    this.EntryName = "Upload complete";

                }

                _ = ResetStatusTask(curSyncCounter);
            });
        }
    }
}
