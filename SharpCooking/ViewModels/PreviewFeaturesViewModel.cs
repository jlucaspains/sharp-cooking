using System.Threading.Tasks;
using SharpCooking.Localization;
using SharpCooking.Services;

namespace SharpCooking.ViewModels
{
    public class PreviewFeaturesViewModel : BaseViewModel
    {
        private readonly IEssentials _essentials;

        public PreviewFeaturesViewModel(IEssentials essentials)
        {
            Title = Resources.PreviewFeaturesTitle;
            _essentials = essentials;
        }

        public bool FocusModeIsActive { get; set; } = true;

        public override Task InitializeAsync()
        {
            FocusModeIsActive = _essentials.GetBoolSetting(AppConstants.PreviewFeatureFocusMode);

            return Task.CompletedTask;
        }

#pragma warning disable CA1801 // Review unused parameters
        public void OnPropertyChanged(string propertyName, object before, object after)
#pragma warning restore CA1801 // Review unused parameters
        {
            if (propertyName == nameof(FocusModeIsActive))
                _essentials.SetBoolSetting(AppConstants.PreviewFeatureFocusMode, FocusModeIsActive);
        }
    }
}