using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SharpCooking.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly IEssentials _essentials;
        private readonly IHttpService _httpService;

        public AboutViewModel(IEssentials essentials, IHttpService httpService)
        {
            Title = Resources.AboutView_Title;
            _essentials = essentials;
            _httpService = httpService;
        }

        public override async Task InitializeAsync()
        {
            AppVersion = _essentials.GetVersion();

            if (IsReleaseNotesLoaded)
                return;

            try
            {
                IsBusy = true;

                var url = string.Format(AppConstants.ReleaseNotesConfigUrl, CultureInfo.CurrentUICulture.Name.Replace("-", ""));
                var result = await _httpService.GetAsync<ReleaseNotesItem[]>(url);
                var thisVersion = result?.FirstOrDefault(item => item.Version == AppVersion);

                NewInVersion = thisVersion?.New ?? new string[] { };
                KnownIssues = thisVersion?.KnownIssues ?? new string[] { };
                IsReleaseNotesLoaded = true;
            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }
            finally
            {
                IsBusy = false;
            }

            await base.InitializeAsync();
        }

        public string AppVersion { get; set; }
        public string[] NewInVersion { get; set; } = new string[] { };
        public bool ShowNewInVersion => NewInVersion.Length > 0;
        public string[] KnownIssues { get; set; } = new string[] { };
        public bool ShowKnownIssues => KnownIssues.Length > 0;
        public bool IsReleaseNotesLoaded { get; set; }
    }
}