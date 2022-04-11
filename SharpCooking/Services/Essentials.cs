using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.StoreReview;
using Xamarin.Essentials;

namespace SharpCooking.Services
{
#pragma warning disable CA1724
    public class Essentials : IEssentials
#pragma warning restore CA1724
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

        public void SetStringSetting(string key, string value)
        {
            Preferences.Set(key, value);
        }

        public bool GetBoolSetting(string key)
        {
            return Preferences.Get(key, false);
        }

        public void SetBoolSetting(string key, bool value)
        {
            Preferences.Set(key, value);
        }

        public async Task<bool> LaunchUri(string uri)
        {
            await Launcher.OpenAsync(new Uri(uri));
            return true;
        }

        public int GetIntSetting(string key)
        {
            return Preferences.Get(key, 0);
        }

        public void SetIntSetting(string key, int value)
        {
            Preferences.Set(key, value);
        }

        public bool IsFirstLaunchForCurrentBuild()
        {
            return VersionTracking.IsFirstLaunchForCurrentBuild;
        }

        public bool IsFirstLaunchEver()
        {
            return VersionTracking.IsFirstLaunchEver;
        }

        public async Task ShareText(string text, string title)
        {
            await Share.RequestAsync(new ShareTextRequest(text, title));
        }

        public async Task ShareFile(string filePath, string title)
        {
            var file = new Xamarin.Essentials.ReadOnlyFile(filePath);
            await Share.RequestAsync(new ShareFileRequest(title, file));
        }

        public void KeepScreenOn(bool state)
        {
            DeviceDisplay.KeepScreenOn = state;
        }

        public bool GetKeepScreenOn()
        {
            return DeviceDisplay.KeepScreenOn;
        }

        public async Task RequestReview()
        {
#if DEBUG
            var test = true;
#else
            var test = false;
#endif

            await CrossStoreReview.Current.RequestReview(test);
        }

        public void OpenStoreListing()
        {
            var appId = DeviceInfo.Platform == DevicePlatform.Android
                      ? "com.lpains.sharpcooking"
                      : "1522623942";
            CrossStoreReview.Current.OpenStoreReviewPage(appId);
        }

        public async Task<(bool Success, string FileName, Stream data)> PickFile(params string[] fileType)
        {
            FileData fileData = await CrossFilePicker.Current.PickFile(fileType);
            if (fileData == null)
                return (false, null, null); // user canceled file picking

            return (true, fileData.FileName, fileData.GetStream());
        }

        public async Task SpeakAsync(string speech, CancellationToken cancellationToken = default)
        {
            await TextToSpeech.SpeakAsync(speech, cancellationToken);
        }
    }
}
