using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaPlanner.Pages;
using AvaloniaPlanner.Views;
using CSUtil.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Controls
{
    public class ProjectsSyncControl : PaneEntry
    {
        const string DEFAULT_NAME = "Projects sync";
        const string SUCCESS_NAME = "Projects synced";

        const Material.Icons.MaterialIconKind DEFAULT_ICON = Material.Icons.MaterialIconKind.Sync;
        const Material.Icons.MaterialIconKind SUCCESS_ICON = Material.Icons.MaterialIconKind.Check;

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
                var ret = await Api.Post<object>("api/project/update_user_projects", "data".ToApiParam(data));
                isWaiting = false;

                await delayTask;
                syncCounter++;
                var curSyncCounter = syncCounter;
                syncInProgress = false;
                if (!ret)
                {
                    this.Icon = Material.Icons.MaterialIconKind.SyncAlert;
                    this.ViewModel.IconBrush = FAILURE_BRUSH;
                    this.EntryName = ret.Message!;
                }
                else
                {
                    this.Icon = SUCCESS_ICON;
                    this.ViewModel.IconBrush = SUCCESS_BRUSH;
                    this.EntryName = SUCCESS_NAME;

                    await Task.Delay(3000);
                    if(curSyncCounter == syncCounter)
                    {
                        this.Icon = DEFAULT_ICON;
                        this.ViewModel.IconBrush = DEFAULT_BRUSH;
                        this.EntryName = DEFAULT_NAME;
                    }
                }
            });
        }
    }
}
