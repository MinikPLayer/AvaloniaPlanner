using Avalonia.Controls;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.Views;
using AvaloniaPlannerLib.Data.Auth;
using CSUtil.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Utils
{
    public class SettingsSync
    {
        public static string? SettingsSyncToken = null;

        static SettingsSync()
        {
            Api.port = 5072;
        }

        public static async Task Login()
        {
            var dialog = new LoginDialog();
            await MainView.OpenDialog(dialog);

            if (string.IsNullOrEmpty(dialog.Login) || string.IsNullOrEmpty(dialog.Password))
                return;

            var login = dialog.Login;
            var password = dialog.Password;

            var result = await Api.Post<ApiAuthToken>("api/Auth/login", "login".ToApiParam(login), "password".ToApiParam(password));
            if (result && result.Payload != null)
                SettingsSyncToken = result.Payload.Token;
        }

        public static async Task Register()
        {
            var dialog = new RegisterDialog();
            await MainView.OpenDialog(dialog);

            if (string.IsNullOrEmpty(dialog.Login) || string.IsNullOrEmpty(dialog.Password) || string.IsNullOrEmpty(dialog.Username) || string.IsNullOrEmpty(dialog.Email))
                return;

            var login = dialog.Login;
            var password = dialog.Password;
            var username = dialog.Username;
            var email = dialog.Email;

            var result = await Api.Post<ApiAuthToken>("api/Auth/register", 
                "login".ToApiParam(login), "password".ToApiParam(password), "username".ToApiParam(username), "email".ToApiParam(email));

            if (result && result.Payload != null)
                SettingsSyncToken = result.Payload.Token;
        }
    }
}
