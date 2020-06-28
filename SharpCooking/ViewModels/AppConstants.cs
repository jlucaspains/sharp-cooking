using System.Collections.Generic;

namespace SharpCooking.ViewModels
{
    public class AppConstants
    {
        internal const string TimeBetweenStepsInterval = "TimeBetweenStepsInterval";
        internal const int DefaultTimeBetweenStepsInterval = 5;
        internal const string BackupIsSetup = "BackupIsSetup";
        internal const string StepTimeIdentifierRegex = "(?<Minutes>\\d+)\\s*(minutes|minute|min)|(?<Hours>\\d+)\\s*(hours|hour)|(?<Days>\\d+)\\s*(days|day)";
        internal const string IngredientQuantityRegex = "(?<Fraction>\\d+\\/\\d+)|(?<Regular>\\d+)";
        internal const string SupportUri = "https://sharpcooking.net";
        internal const string Language = "DisplayLanguage";
        internal const string PrivacyPolicyUrl = "https://sharpcooking.net/privacypolicy";
        internal const string DropBoxAccessToken = "DropBoxAccessToken";
        internal static string[] BackupFileMimeTypes = new string[] { "application/zip", "com.pkware.zip-archive" };
        internal const string BackupRecipeFileName = "SharpBackup_Recipe.json";
        internal const string BackupZipFileName = "SharpCooking.zip";
        internal const string BackupRestoreFilePrefix= "SharpRestore";
    }
}