using AvaloniaPlanner.Utils;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaPlanner.ViewModels
{
    public class SettingsViewModel : ReactiveObject
    {
        private bool isLocked = false;
        public bool IsLocked
        {
            get => isLocked;
            set => this.RaiseAndSetIfChanged(ref isLocked, value);
        }

        private bool isLoggedIn = false;
        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set => this.RaiseAndSetIfChanged(ref isLoggedIn, value);
        }

        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand { get; set; }

        public SettingsViewModel()
        {
            IsLoggedIn = SettingsSync.SettingsSyncToken != null;

            LoginCommand = ReactiveCommand.Create(async () =>
            {
                IsLocked = true;
                await SettingsSync.Login();
                IsLocked = false;
                IsLoggedIn = SettingsSync.SettingsSyncToken != null;
            });

            RegisterCommand = ReactiveCommand.Create(async () =>
            {
                IsLocked = true;
                await SettingsSync.Register();
                IsLocked = false;
                IsLoggedIn = SettingsSync.SettingsSyncToken != null;
            });
        }
    }
}
