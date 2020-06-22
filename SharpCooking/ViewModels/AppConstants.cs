namespace SharpCooking.ViewModels
{
    public class AppConstants
    {
        public const string TimeBetweenStepsInterval = "TimeBetweenStepsInterval";
        public const int DefaultTimeBetweenStepsInterval = 5;
        public const string BackupIsSetup = "BackupIsSetup";
        public const string StepTimeIdentifierRegex = "(?<Minutes>\\d+)\\s*(minutes|minute|min)|(?<Hours>\\d+)\\s*(hours|hour)|(?<Days>\\d+)\\s*(days|day)";
        public const string IngredientQuantityRegex = "\\d+";
        public const string SupportUri = "https://sharpcooking.org";
    }
}