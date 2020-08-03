using SharpCooking.Localization;
using SharpCooking.Services;
using System.Threading.Tasks;

namespace SharpCooking.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly IEssentials _essentials;

        public AboutViewModel(IEssentials essentials)
        {
            Title = Resources.AboutView_Title;
            _essentials = essentials;
        }

        public override Task InitializeAsync()
        {
            AppVersion = _essentials.GetVersion();

            return base.InitializeAsync();
        }

        public string AppVersion { get; set; }
    }
}