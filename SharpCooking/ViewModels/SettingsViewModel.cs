using SharpCooking.Localization;
using SharpCooking.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IEssentials _essentials;

        public SettingsViewModel(IEssentials essentials)
        {
            Title = "Settings";
            StepIntervalCommand = new Command(async () => await AdjustStepInterval());
            SetupBackupCommand = new Command(async () => await SetupBackup());
            GetHelpCommand = new Command(async () => await GetHelp());

            _essentials = essentials;
        }

        public ICommand StepIntervalCommand { get; }
        public ICommand SetupBackupCommand { get; }
        public ICommand GetHelpCommand { get; }

        public int TimeBetweenStepsInterval { get; set; }
        public string DisplayTimeBetweenStepsInterval { get { return $"{TimeBetweenStepsInterval} {Resources.SettingsView_TimeStepsDescriptoin}"; } }

        public bool BackupIsSetup { get; set; }
        public string DisplayBackupOption { get { return BackupIsSetup ? $"{Resources.SettingsView_BackingUpTo} dropbox" : Resources.SettingsView_SetupBackup; } }

        public override Task InitializeAsync()
        {
            TimeBetweenStepsInterval = _essentials.GetIntSetting(AppConstants.TimeBetweenStepsInterval);
            BackupIsSetup = _essentials.GetBoolSetting(AppConstants.BackupIsSetup);

            if (TimeBetweenStepsInterval == 0)
                TimeBetweenStepsInterval = AppConstants.DefaultTimeBetweenStepsInterval;

            return base.InitializeAsync();
        }

        private async Task AdjustStepInterval()
        {
            var result = await DisplayPromptAsync(Resources.SettingsView_StepsIntervalTitle, Resources.SettingsView_StepsIntervalDescription, 
                Resources.SettingsView_StepsIntervalOk, Resources.SettingsView_StepsIntervalCancel, TimeBetweenStepsInterval.ToString(), Keyboard.Numeric);

            if(int.TryParse(result, out int parsedResult) && parsedResult > 0)
            {
                TimeBetweenStepsInterval = parsedResult;
                _essentials.SetIntSetting(AppConstants.TimeBetweenStepsInterval, TimeBetweenStepsInterval);
            }
        }

        private async Task SetupBackup()
        {
            await GoToAsync("backup");
        }

        private async Task GetHelp()
        {
            await _essentials.LaunchUri(AppConstants.SupportUri);
        }
    }
}