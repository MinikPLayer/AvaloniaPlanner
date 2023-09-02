using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPlanner.ViewModels;
using AvaloniaPlanner.Views;
using AvaloniaPlannerLib.Data.Project;
using CSUtil.Reflection;

namespace AvaloniaPlanner.Dialogs
{
    public partial class ProjectBinEditDialog : UserControl
    {
        public static TaskOrderingModes[] TaskOrderingModes { get; } =
            Enum.GetValues<TaskOrderingModes>();

        private ApiProjectBin ogBin { get; set; }
        public bool Save { get; set; } = false;

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && this.DataContext is ProjectBinViewModel vm && c.Tag is string s && s == "Save")
            {
                if (ShownTasksCB.IsChecked.HasValue && ShownTasksCB.IsChecked.Value == false)
                    (this.DataContext as ProjectBinViewModel)!.TaskCountToShow = -1;
                
                // Copy new data to the new task
                ClassCopier.Copy(vm.GetBin(), ogBin);
                Save = true;
            }
            else
            {
                Save = false;
            }

            MainView.Singleton.MainDialog.CloseDialogCommand.Execute(Save);
        }

        public ProjectBinEditDialog(ApiProjectBin bin)
        {
            InitializeComponent();
            if (bin.ShownTaskCount == -1)
            {
                ShownTasksCB.IsChecked = false;
                bin.ShownTaskCount = 0;
            }
            else
            {
                ShownTasksCB.IsChecked = true;
            }

            this.ogBin = bin;
            this.DataContext = new ProjectBinViewModel(ClassCopier.Create<ApiProjectBin>(bin));           
        }
    }
}
