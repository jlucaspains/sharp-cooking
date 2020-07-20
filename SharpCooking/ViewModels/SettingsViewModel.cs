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

        public SettingsViewModel(IEssentials essentials, IDataStore store, IConnectionFactory connectionFactory)
        {
            Title = Resources.AppShell_Settings;
            StepIntervalCommand = new Command(async () => await AdjustStepInterval());
            BackupCommand = new Command(async () => await Backup());
            RestoreBackupCommand = new Command(async () => await RestoreBackup());
            GetHelpCommand = new Command(async () => await GetHelp());
            ChangeCultureCommand = new Command(async () => await ChangeCulture());
            ViewPrivacyPolicyCommand = new Command(async () => await ViewPrivacyPolicy());
            MultiplierResultFormatCommand = new Command(() => AdjustMultiplierResultFormat());

            _essentials = essentials;
            _store = store;
            _connectionFactory = connectionFactory;
        }

        public Command StepIntervalCommand { get; }
        public Command BackupCommand { get; }
        public Command RestoreBackupCommand { get; }
        public Command GetHelpCommand { get; }
        public Command ChangeCultureCommand { get; }
        public Command ViewPrivacyPolicyCommand { get; }
        public Command MultiplierResultFormatCommand { get; }

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
                Resources.SettingsView_StepsIntervalOk, Resources.SettingsView_StepsIntervalCancel, TimeBetweenStepsInterval.ToString(), Keyboard.Numeric);

            if (int.TryParse(result, out int parsedResult) && parsedResult > 0)
            {
                TimeBetweenStepsInterval = parsedResult;
                _essentials.SetIntSetting(AppConstants.TimeBetweenStepsInterval, TimeBetweenStepsInterval);
            }
        }

        async Task Backup()
        {
            var appFolder = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var allRecipes = await _store.AllAsync<Recipe>();

            var recipesFile = Path.Combine(appFolder, AppConstants.BackupRecipeFileName);
            var allFiles = allRecipes.Where(item => !string.IsNullOrEmpty(item.MainImagePath)).Select(item => item.MainImagePath).ToList();

            // remove the folder path out of the main image path
            foreach (var item in allRecipes)
                item.MainImagePath = Path.GetFileName(item.MainImagePath);

            var recipesJson = JsonConvert.SerializeObject(allRecipes);
            File.WriteAllText(recipesFile, recipesJson);

            allFiles.Add(recipesFile);

            var zipPath = Path.Combine(appFolder, AppConstants.BackupZipFileName);

            QuickZip(allFiles.ToArray(), zipPath);

            await _essentials.ShareFile(zipPath, Resources.SettingsView_PickBackupLocation);
        }

        async Task RestoreBackup()
        {
            var result = await _essentials.PickFile(AppConstants.BackupFileMimeTypes);

            if (!result.Success)
            {
                await DisplayAlertAsync(Resources.SettingsView_RestoreCancelled, Resources.SettingsView_ResourceCancelledContent, Resources.ErrorOk);
                return;
            }

            using (result.data)
            {
                using (ZipArchive zip = new ZipArchive(result.data, ZipArchiveMode.Read))
                {
                    if (!zip.Entries.Any(item => item.Name == AppConstants.BackupRecipeFileName))
                    {
                        await DisplayAlertAsync(Resources.ErrorTitle, Resources.SettingsView_BadBackupFile, Resources.ErrorOk);
                        return;
                    }

                    var appFolder = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                    IEnumerable<Recipe> restoreRecipes = null;

                    try
                    {
                        var entry = zip.GetEntry(AppConstants.BackupRecipeFileName);
                        var stream = entry.Open();

                        using (StreamReader reader = new StreamReader(stream))
                            restoreRecipes = JsonConvert.DeserializeObject<IEnumerable<Recipe>>(await reader.ReadToEndAsync());
                    }
                    catch
                    {
                        await DisplayAlertAsync(Resources.ErrorTitle, Resources.SettingsView_CorruptedBackupFile, Resources.ErrorOk);
                        return;
                    }

                    if (restoreRecipes == null || !restoreRecipes.Any())
                    {
                        await DisplayAlertAsync(Resources.ErrorTitle, Resources.SettingsView_NoRecipesToRestore, Resources.ErrorOk);
                        return;
                    }

                    var originalFiles = Directory.GetFiles(appFolder);

                    foreach (var originalFile in originalFiles)
                    {
                        var fileName = Path.GetFileName(originalFile);

                        if (fileName.StartsWith(AppConstants.BackupRestoreFilePrefix))
                            File.Delete(originalFile);
                    }

                    // rename existing files
                    foreach (var originalFile in originalFiles)
                    {
                        var fileName = Path.GetFileName(originalFile);

                        if (fileName.StartsWith(AppConstants.BackupRestoreFilePrefix) || fileName.EndsWith(".db3"))
                            continue;
                        else
                            File.Move(originalFile,
                                originalFile.Replace(originalFile, $"{Path.GetDirectoryName(originalFile)}{Path.DirectorySeparatorChar}{AppConstants.BackupRestoreFilePrefix}_{fileName}"));
                    }

                    // drop all files in place
                    zip.ExtractToDirectory(appFolder);

                    //remove old data
                    var allRecipes = await _store.AllAsync<Recipe>();
                    foreach (var recipe in allRecipes)
                        await _store.DeleteAsync(recipe);

                    // add new data
                    foreach (var recipe in restoreRecipes)
                    {
                        recipe.Id = 0;
                        recipe.MainImagePath = recipe.MainImagePath == null
                            ? null
                            : Path.Combine(appFolder, Path.GetFileName(recipe.MainImagePath));

                        await _store.InsertAsync(recipe);
                    }

                    await DisplayToastAsync(Resources.SettingsView_RestoreIsDone);
                }
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

        void AdjustMultiplierResultFormat()
        {
            UseFractions = !UseFractions;
            _essentials.SetBoolSetting(AppConstants.MultiplierResultUseFractions, UseFractions);
        }

        bool QuickZip(string[] filesToZip, string destinationZipFullPath)
        {
            try
            {
                // Delete existing zip file if exists
                if (File.Exists(destinationZipFullPath))
                    File.Delete(destinationZipFullPath);

                using (ZipArchive zip = ZipFile.Open(destinationZipFullPath, ZipArchiveMode.Create))
                {
                    foreach (var file in filesToZip)
                    {
                        zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    }
                }

                return File.Exists(destinationZipFullPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                return false;
            }
        }
    }
}