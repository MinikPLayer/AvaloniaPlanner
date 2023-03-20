using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.ViewModels;
using DialogHostAvalonia;
using System;
using System.Diagnostics;

namespace AvaloniaPlanner.Controls
{
    public partial class ProjectTaskControl : UserControl
    {
        private bool dialogOpened = false;
        private void DialogClosed(object sender, DialogClosingEventArgs e)
        {
            dialogOpened = false;
            var result = e.Parameter;
            if (result is bool b && b == true && e.Session.Content is ProjectTaskEditDialog dialog)
                this.DataContext = dialog.DataContext;
        }

        public void StackPanelPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed || this.DataContext is not ProjectTaskViewModel vm)
                return;

            if (dialogOpened)
                return;

            dialogOpened = true;
            DialogHost.Show(new ProjectTaskEditDialog(vm.GetTaskCopy()), closingEventHandler: DialogClosed);
        }

        public ProjectTaskControl()
        {
            InitializeComponent();
        }
    }
}
