using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SharpCooking.Services
{
    public interface IEssentials
    {
        string GetVersion();
        void ShowSettings();
        string GetStringSetting(string key);
        bool GetBoolSetting(string key);
        int GetIntSetting(string key);
        void SetStringSetting(string key, string value);
        void SetIntSetting(string key, int value);
        void SetBoolSetting(string key, bool value);
        Task<bool> LaunchUri(string uri);
        Task ShareText(string text, string title);
    }

    public class Essentials : IEssentials
    {
        public string GetVersion()
        {
            return AppInfo.VersionString;
        }

        public void ShowSettings()
        {
            AppInfo.ShowSettingsUI();
        }

        public string GetStringSetting(string key)
        {
            return Preferences.Get(key, null);
        }

        public bool GetBoolSetting(string key)
        {
            return Preferences.Get(key, false);
        }

        public void SetStringSetting(string key, string value)
        {
            Preferences.Set(key, value);
        }

        public void SetBoolSetting(string key, bool value)
        {
            Preferences.Set(key, value);
        }

        public async Task<bool> LaunchUri(string uri)
        {
            return await Launcher.TryOpenAsync(uri);
        }

        public int GetIntSetting(string key)
        {
            return Preferences.Get(key, 0);
        }

        public void SetIntSetting(string key, int value)
        {
            Preferences.Set(key, value);
        }

        public async Task ShareText(string text, string title)
        {
            await Share.RequestAsync(new ShareTextRequest(text, title));
        }
    }
}
