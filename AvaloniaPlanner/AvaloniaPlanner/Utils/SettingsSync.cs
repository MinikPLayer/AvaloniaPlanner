using Avalonia.Controls;
using AvaloniaPlanner.Dialogs;
using AvaloniaPlanner.Views;
using AvaloniaPlannerLib.Data.Auth;
using CSUtil.Crypto;
using CSUtil.Logging;
using CSUtil.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvaloniaPlanner.Pages.SettingsPage;

namespace AvaloniaPlanner.Utils
{
    public class SettingsSync
    {
        static string DefaultTokenSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvPlanner", "avplanner");

        static void SaveSyncToken(string? token)
        {
            if (token == null)
            {
                if (File.Exists(DefaultTokenSavePath))
                    File.Delete(DefaultTokenSavePath);
                return;
            }

            var path = Path.GetDirectoryName(DefaultTokenSavePath);
            if (path == null)
                throw new Exception("Invalid path, cannot extract a directory - " + DefaultTokenSavePath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var keys = AesCryptor.GenerateRandomKey();
            var data = AesCryptor.Encrypt(token, keys.key, keys.iv);
            var finalData = new List<byte>(keys.key.Length + keys.iv.Length + data.Length);
            finalData.AddRange(keys.key);
            finalData.AddRange(keys.iv);
            finalData.AddRange(data);

            File.WriteAllBytes(DefaultTokenSavePath, finalData.ToArray());
        }

        public static bool TryLoadSyncToken()
        {
            if(File.Exists(DefaultTokenSavePath))
            {
                var bytes = File.ReadAllBytes(DefaultTokenSavePath);
                if (bytes.Length < 32)
                    return false;

                var key = new byte[16];
                var iv = new byte[16];
                var data = new byte[bytes.Length - 32];

                Array.Copy(bytes, key, 16);
                Array.Copy(bytes, 16, iv, 0, 16);
                Array.Copy(bytes, 32, data, 0, data.Length);

                var token = AesCryptor.DecryptString(data, key, iv);
                SettingsSyncToken = token;
                return true;
            }

            return false;
        }

        static string? _settingsSyncToken = null;
        public static string? SettingsSyncToken
        {
            get => _settingsSyncToken;
            set
            {
                _settingsSyncToken = value;
                Api.token = value;
                SaveSyncToken(value);
            }
        }

        public static async Task<bool> TestConnection()
        {
            var result = await Api.Get<string>("api/Auth/test_connection");
            return (bool)result;
        }

        public static async Task<string?> TestLogin()
        {
            var result = await Api.Get<string>("api/Auth/get_user_id");
            if (result.IsOk())
                return result.Payload;
            else
                return null;
        }

        public static void Logout()
        {
            SettingsSyncToken = null;
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
            if (result.IsOk() && result.Payload != null)
                SettingsSyncToken = result.Payload.Token;
            else
                SettingsSyncToken = null;
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

            if (result.IsOk() && result.Payload != null)
                SettingsSyncToken = result.Payload.Token;
            else
                SettingsSyncToken = null;
        }
    }
}
