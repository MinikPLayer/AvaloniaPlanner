using AvaloniaPlanner.Utils;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private string _loginStatus = "";
        public string LoginStatus
        {
            get => _loginStatus;
            set => this.RaiseAndSetIfChanged(ref _loginStatus, value);
        }

        private string _connectionStatus = "";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
        }

        public ICommand LoginCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand TestConnectionCommand { get; set; }

        public SettingsViewModel()
        {
            IsLoggedIn = SettingsSync.SettingsSyncToken != null;

            LoginCommand = ReactiveCommand.Create(async () =>
            {
                IsLocked = true;
                await SettingsSync.Login();
                IsLocked = false;
                IsLoggedIn = SettingsSync.SettingsSyncToken != null;
                LoginStatus = IsLoggedIn.ToString();
            });

            LogoutCommand = ReactiveCommand.Create(() =>
            {
                SettingsSync.Logout();
                IsLoggedIn = SettingsSync.SettingsSyncToken != null;
                LoginStatus = IsLoggedIn.ToString();
            });

            RegisterCommand = ReactiveCommand.Create(async () =>
            {
                IsLocked = true;
                await SettingsSync.Register();
                IsLocked = false;
                IsLoggedIn = SettingsSync.SettingsSyncToken != null;
            });

            TestConnectionCommand = ReactiveCommand.Create(async () =>
            {
                LoginStatus = "";
                ConnectionStatus = "...";
                IsLocked = true;
                var result = await SettingsSync.TestConnection();
                ConnectionStatus = result.ToString();

                LoginStatus = "...";
                var loginResult = await SettingsSync.TestLogin();
                LoginStatus = loginResult == null ? "False" : "True";
                IsLocked = false;
            });
        }
    }
}
