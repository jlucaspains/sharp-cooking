﻿using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SharpCooking.Services
{
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

        public bool IsFirstLaunchForCurrentBuild()
        {
            return VersionTracking.IsFirstLaunchForCurrentBuild;
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

        public async Task<(bool Success, string FileName, Stream data)> PickFile(params string[] fileType)
        {
            FileData fileData = await CrossFilePicker.Current.PickFile(fileType);
            if (fileData == null)
                return (false, null, null); // user canceled file picking

            return (true, fileData.FileName, fileData.GetStream());
        }
    }
}