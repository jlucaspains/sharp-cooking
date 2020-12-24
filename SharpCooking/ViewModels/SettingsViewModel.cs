using Newtonsoft.Json;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IEssentials _essentials;
        private readonly IDataStore _store;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IFileHelper _fileHelper;
        private readonly IRecipePackager _recipePackager;

        public SettingsViewModel(IEssentials essentials, IDataStore store, IConnectionFactory connectionFactory, IFileHelper fileHelper, IRecipePackager recipePackager)
        {
            Title = Resources.AppShell_Settings;
            StepIntervalCommand = new Command(async () => await AdjustStepInterval());
            BackupCommand = new Command(async () => await Backup());
            RestoreBackupCommand = new Command(async () => await RestoreBackup());
            GetHelpCommand = new Command(async () => await GetHelp());
            ChangeCultureCommand = new Command(async () => await ChangeCulture());
            ViewPrivacyPolicyCommand = new Command(async () => await ViewPrivacyPolicy());
            MultiplierResultFormatCommand = new Command(() => AdjustMultiplierResultFormat());
            ViewCreditsCommand = new Command(async () => await ViewCredits());

            _essentials = essentials;
            _store = store;
            _connectionFactory = connectionFactory;
            _fileHelper = fileHelper;
            _recipePackager = recipePackager;
        }

        public Command StepIntervalCommand { get; }
        public Command BackupCommand { get; }
        public Command RestoreBackupCommand { get; }
        public Command GetHelpCommand { get; }
        public Command ChangeCultureCommand { get; }
        public Command ViewPrivacyPolicyCommand { get; }
        public Command MultiplierResultFormatCommand { get; }
        public Command ViewCreditsCommand { get; }

        public int TimeBetweenStepsInterval { get; set; }
        public string DisplayTimeBetweenStepsInterval { get { return $"{TimeBetweenStepsInterval} {Resources.SettingsView_TimeStepsDescriptoin}"; } }

        public bool UseFractions { get; set; }
        public string MultiplierResultDisplay { get { return UseFractions ? Resources.SettingsView_MultiplyResultDisplayUseFractions : Resources.SettingsView_MultiplyResultDisplayUseDecimal; } }

        public string CurrentLanguage { get; set; }

        public override Task InitializeAsync()
        {
            TimeBetweenStepsInterval = _essentials.GetIntSetting(AppConstants.TimeBetweenStepsInterval);

            if (TimeBetweenStepsInterval == 0)
                TimeBetweenStepsInterval = AppConstants.DefaultTimeBetweenStepsInterval;

            UseFractions = _essentials.GetBoolSetting(AppConstants.MultiplierResultUseFractions);

            CurrentLanguage = CultureInfo.CurrentUICulture.DisplayName;

            return base.InitializeAsync();
        }

        async Task AdjustStepInterval()
        {
            var result = await DisplayPromptAsync(Resources.SettingsView_StepsIntervalTitle, Resources.SettingsView_StepsIntervalDescription,
                Resources.SettingsView_StepsIntervalOk, Resources.SettingsView_StepsIntervalCancel, TimeBetweenStepsInterval.ToString(CultureInfo.CurrentCulture), Keyboard.Numeric);

            if (int.TryParse(result, out int parsedResult) && parsedResult > 0)
            {
                TimeBetweenStepsInterval = parsedResult;
                _essentials.SetIntSetting(AppConstants.TimeBetweenStepsInterval, TimeBetweenStepsInterval);
            }
        }

        async Task Backup()
        {
            try
            {
                using (DisplayLoading(Resources.SettingsView_CreatingBackup))
                {
                    var allRecipes = await _store.AllAsync<Recipe>();

                    var packagePath = await _recipePackager.PackageRecipes(allRecipes);

                    await _essentials.ShareFile(packagePath, Resources.SettingsView_PickBackupLocation);
                }
            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }
        }

        async Task RestoreBackup()
        {
            try
            {
                var filePickResult = await _essentials.PickFile(AppConstants.BackupFileMimeTypes);

                if (!filePickResult.Success)
                {
                    await DisplayAlertAsync(Resources.SettingsView_RestoreCancelled, Resources.SettingsView_ResourceCancelledContent, Resources.ErrorOk);
                    return;
                }

                using (DisplayLoading(Resources.SettingsView_RestoringBackup))
                {
                    using (filePickResult.data)
                    {
                        var packResult = await _recipePackager.RestoreRecipePackage(filePickResult.data, true);

                        if (!packResult.Succeeded)
                            await DisplayAlertAsync(Resources.ErrorTitle, packResult.Error, Resources.ErrorOk);
                        else
                            await DisplayToastAsync(Resources.SettingsView_RestoreIsDone);
                    }
                }
            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }
        }

        async Task GetHelp()
        {
            await _essentials.LaunchUri(AppConstants.SupportUri);
        }

        async Task ChangeCulture()
        {
            await DisplayAlertAsync(Resources.SettingsView_NotSupported, Resources.SettingsView_CannotChangeCulture, Resources.ErrorOk);
        }

        async Task ViewPrivacyPolicy()
        {
            await _essentials.LaunchUri(AppConstants.PrivacyPolicyUrl);
        }

        async Task ViewCredits()
        {
            await GoToAsync("credits");
        }

        void AdjustMultiplierResultFormat()
        {
            UseFractions = !UseFractions;
            _essentials.SetBoolSetting(AppConstants.MultiplierResultUseFractions, UseFractions);
        }

        async Task<bool> QuickZip(string[] filesToZip, string destinationZipFullPath)
        {
            try
            {
                // Delete existing zip file if exists
                if (await _fileHelper.ExistsAsync(destinationZipFullPath))
                    await _fileHelper.DeleteAsync(destinationZipFullPath);

                await Task.Run(() =>
                {
                    using (ZipArchive zip = ZipFile.Open(destinationZipFullPath, ZipArchiveMode.Create))
                    {
                        foreach (var file in filesToZip)
                        {
                            zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                        }
                    }
                });

                return await _fileHelper.ExistsAsync(destinationZipFullPath);
            }
            catch (Exception ex)
            {
                await TrackException(ex);
                return false;
            }
        }
    }
}