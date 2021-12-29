namespace SharpCooking.ViewModels
{
    public static class AppConstants
    {
        internal const string TimeBetweenStepsInterval = "TimeBetweenStepsInterval";
        internal const string MultiplierResultUseFractions = "MultiplierResultUseFractions";
        internal const int DefaultTimeBetweenStepsInterval = 5;
        internal const string SupportUri = "http://sharpcooking.net/documentation";
        internal const string Language = "DisplayLanguage";
        internal const string PrivacyPolicyUrl = "http://sharpcooking.net/privacypolicy";
        internal const string DropBoxAccessToken = "DropBoxAccessToken";
        internal static string[] BackupFileMimeTypes = new string[] { "application/zip", "com.pkware.zip-archive" };
        internal const string BackupRecipeFileName = "SharpBackup_Recipe.json";
        internal const string BackupZipFileName = "SharpCooking.zip";
        internal const string BackupRestoreFilePrefix= "SharpRestore";
        internal const string ImportConfigurationFileName = "importConfiguration.json";
        internal const string RecipeDownloadConfigVersionSetting = "RecipeDownloadConfigVersion";
        internal const string FocusModeIsNarrationEnabled = "FocusModeIsNarrationEnabled";
        internal const string PreviewFeatureFocusMode = "PreviewFeature_FocusMode";
        internal const string RecipeDownloadConfigVersionUrl = "https://raw.githubusercontent.com/jlucaspains/sharp-cooking-pages/master/assets/config/recipedownloadversion.txt";
        internal const string RecipeDownloadConfigUrl = "https://raw.githubusercontent.com/jlucaspains/sharp-cooking-pages/master/assets/config/recipedownload.json";
        internal const string ReleaseNotesConfigUrl = "https://raw.githubusercontent.com/jlucaspains/sharp-cooking-pages/master/_data/releasenotes_{0}.json";
        internal const string CreditsDownloadUrl = "https://raw.githubusercontent.com/jlucaspains/sharp-cooking-pages/master/_data/credits.json";
    }
}