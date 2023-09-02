using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Styling;
using ReactiveUI;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaPlanner.Controls
{
    public class MessageDialogEventArgs : EventArgs
    {
        public object? Parameter { get; set; }
        public object? Content { get; set; }
    }

    public class MessageDialogViewModel : ReactiveObject
    {
        private object? dialogContent = new TextBlock() { Text = "TEST" };
        public object? DialogContent
        {
            get => dialogContent;
            set => this.RaiseAndSetIfChanged(ref dialogContent, value);
        }

        private object? child = new TextBlock() { Text = "Undefined object" };
        public object? Child
        {
            get => child;
            set => this.RaiseAndSetIfChanged(ref child, value);
        }

        private bool isOpen = false;
        public bool IsOpen
        {
            get => isOpen;
            set => this.RaiseAndSetIfChanged(ref isOpen, value);
        }

        public double TargetDialogOpacity = 1;

        private double dialogOpacity = 0;
        public double DialogOpacity
        {
            get => dialogOpacity;
            set => this.RaiseAndSetIfChanged(ref dialogOpacity, value);
        }

        public async Task RunAnimation(double target)
        {
            TargetDialogOpacity = target;
            const int steps = 10;
            var step = (TargetDialogOpacity - dialogOpacity) / steps;
            for (var i = 0; i < steps; i++)
            {
                DialogOpacity += step;
                await Task.Delay(5);
            }
            DialogOpacity = target;
        }
    }

    public partial class MessageDialog : UserControl
    {
        private MessageDialogViewModel ViewModel => (MessageDialogViewModel)this.DataContext!;

        public static readonly StyledProperty<object?> DialogContentProperty =
            AvaloniaProperty.Register<MessageDialog, object?>(nameof(DialogContent));

        public object? DialogContent
        {
            get { return GetValue(DialogContentProperty); }
            set
            {
                SetValue(DialogContentProperty, value);
                (this.DataContext as MessageDialogViewModel)!.DialogContent = value;
            }
        }

        public static readonly StyledProperty<object?> ChildProperty =
            AvaloniaProperty.Register<MessageDialog, object?>(nameof(Child));

        public object? Child
        {
            get { return GetValue(ChildProperty); }
            set
            {
                SetValue(ChildProperty, value);
                (this.DataContext as MessageDialogViewModel)!.Child = value;
            }
        }

        AutoResetEvent taskClosedEvent = new AutoResetEvent(false);
        public async Task ShowDialog(object content, Action<object, MessageDialogEventArgs>? handler)
        {
            DialogContent = content;
            ViewModel.IsOpen = true;
            await ViewModel.RunAnimation(1.0);

            object? dialogValue = null;
            this.CloseDialogCommand = ReactiveCommand.Create<object>((o) => {
                dialogValue = o;
                taskClosedEvent.Set();
            });
            await Task.Run(taskClosedEvent.WaitOne);
            if (handler != null)
                handler(this, new MessageDialogEventArgs() { Content = content, Parameter = dialogValue });

            await ViewModel.RunAnimation(0.0);
            ViewModel.IsOpen = false;
        }

        public ICommand CloseDialogCommand { get; set; }

        public MessageDialog()
        {
            this.DataContext = new MessageDialogViewModel();
            InitializeComponent();
        }
    }
}
