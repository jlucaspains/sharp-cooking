using System;
using System.IO;
using System.Threading.Tasks;

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
        bool IsFirstLaunchEver();
        bool IsFirstLaunchForCurrentBuild();
        Task<bool> LaunchUri(string uri);
        Task ShareText(string text, string title);
        Task ShareFile(string filePath, string title);
        Task<(bool Success, string FileName, Stream data)> PickFile(params string[] fileType);
    }
}
