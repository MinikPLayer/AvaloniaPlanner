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
        private ApiProjectBin ogBin { get; set; }
        public bool Save { get; set; } = false;

        public void CloseDialog(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && this.DataContext is ProjectBinViewModel vm && c.Tag is string s && s == "Save")
            {
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
            this.ogBin = bin;
            this.DataContext = new ProjectBinViewModel(ClassCopier.Create<ApiProjectBin>(bin));           
        }
    }
}
