using Newtonsoft.Json;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using SharpCooking.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.DateTimePopups;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    [QueryProperty("Id", "id")]
    public class ItemDetailViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;
        private readonly IEssentials _essentials;
        private readonly IRecipePackager _recipePackager;
        private readonly ISpeechRecognizer _speechRecognizer;

        public RecipeViewModel Item { get; set; }
        public Recipe Model { get; set; }

        public string Id { get; set; }

        public Command EditCommand { get; }
        public Command ChangeMultiplierCommand { get; }
        public Command MoreCommand { get; }
        public Command ShareRecipeCommand { get; }
        public Command ChangeStartTimeCommand { get; }
        public Command ToggleKeepScreenOnCommand { get; }
        public Command ActivateCookingModeCommand { get; }

        public ObservableCollection<StepViewModel> Steps { get; } = new ObservableCollection<StepViewModel>();
        public decimal Multiplier { get; set; }
        public string MultiplierDisplay { get; set; }
        public int StandardStepTimeInterval { get; set; }
        public bool HasNotes { get { return !string.IsNullOrEmpty(Item?.Notes); } }
        public bool HasSource { get { return !string.IsNullOrEmpty(Item?.Source); } }
        public bool DoesNotHaveMainImage { get { return string.IsNullOrEmpty(Item?.MainImagePath); } }
        public bool HasMainImage { get { return !string.IsNullOrEmpty(Item?.MainImagePath); } }
        public bool KeepScreenOn { get; set; }
        public string ToggleScreenIcon { get { return KeepScreenOn ? IconFont.Cellphone : IconFont.CellphoneLock; } }

        public ItemDetailViewModel(IDataStore dataStore, IEssentials essentials, IRecipePackager recipePackager, ISpeechRecognizer speechRecognizer)
        {
            _dataStore = dataStore;
            _essentials = essentials;
            _recipePackager = recipePackager;
            _speechRecognizer = speechRecognizer;

            EditCommand = new Command(async () => await GotoEdit());
            ChangeMultiplierCommand = new Command(async () => await ChangeMultiplier());
            MoreCommand = new Command(async () => await ShowMoreOptions());
            ShareRecipeCommand = new Command(async () => await ShareRecipeText());
            ChangeStartTimeCommand = new Command(async () => await ChangeStartTime());
            ToggleKeepScreenOnCommand = new Command(async () => await ToggleKeepScreenOn());
            ActivateCookingModeCommand = new Command(async () => await ActivateCookingMode());
        }

        public override async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                KeepScreenOn = _essentials.GetKeepScreenOn();

                if (!int.TryParse(Id, out int parsedId))
                {
                    await ReportError(Resources.ItemDetailView_FailedToParseInputId);
                    return;
                }

                var recipe = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);
                Model = recipe;
                Item = RecipeViewModel.FromModel(recipe);

                Multiplier = recipe.Multiplier == 0 ? 1 : recipe.Multiplier;
                PrepareRecipeToDisplay(Item);

                if (Multiplier != 1)
                    await ChangeMultiplier(recipe.Multiplier);

                IsBusy = false;
            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }

            await base.InitializeAsync();
        }

        public override Task TerminateAsync()
        {
            _essentials.KeepScreenOn(false);

            return Task.CompletedTask;
        }

        void PrepareRecipeToDisplay(RecipeViewModel recipe, DateTime? proposedStart = null)
        {
            var configInterval = _essentials.GetIntSetting(AppConstants.TimeBetweenStepsInterval);

            Title = recipe?.Title;
            StandardStepTimeInterval = configInterval <= 0 ? 5 : configInterval;

            var start = proposedStart ?? DateTime.Now;

            Steps.Clear();
            Steps.Add(new StepViewModel { IsNotLast = true, Time = start.ToShortTimeString(), Title = Resources.ItemDetailView_Ingredients, SubTitle = Item.Ingredients });

            start = start.AddMinutes(StandardStepTimeInterval);

            var instructions = Item.Instructions?.Split(new string[] { "\r\n", "\n\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)?.Where(Item => !string.IsNullOrEmpty(Item))?.Select((item, i) => (i, item))
                ?? Array.Empty<(int i, string item)>();

            var previous = start;

            Func<DateTime, DateTime, string> getDisplayTime = (st, pv) =>
            {
                var timeDiffInDays = st.DayOfYear - pv.DayOfYear;
                return timeDiffInDays > 0 ? $"{st.ToShortTimeString()} +{timeDiffInDays}{Resources.ItemDetailView_DayAbbreviation}" : st.ToShortTimeString();
            };

            foreach ((int i, string item) in instructions)
            {
                Steps.Add(new StepViewModel { IsNotLast = true, Title = $"{Resources.ItemDetailView_Step} {i + 1}", SubTitle = item, Time = getDisplayTime(start, previous) });

                previous = start;

                var timeSpan = Helpers.GetImpliedTimeFromString(item, Resources.StepTimeIdentifierRegex);

                if (timeSpan == TimeSpan.Zero)
                    timeSpan = TimeSpan.FromMinutes(StandardStepTimeInterval);

                start += timeSpan;
            }

            Steps.Add(new StepViewModel { IsNotLast = false, Title = Resources.ItemDetailView_Enjoy, Time = getDisplayTime(start, previous) });
        }

        async Task GotoEdit()
        {
            await GoToAsync("items/edit", new Dictionary<string, object> { { "id", Item.Id } });
        }

        async Task ChangeMultiplier()
        {

            var inputMultiplier = await DisplayPromptAsync(Resources.ItemDetailView_MultiplierTitle, Resources.ItemDetailView_MultiplierDescription,
                Resources.ItemDetailView_MultiplierOk, Resources.ItemDetailView_MultiplierCancel, Multiplier.ToString(CultureInfo.CurrentCulture), Keyboard.Numeric);

            if (decimal.TryParse(inputMultiplier, NumberStyles.Float, CultureInfo.CurrentCulture, out var newMultiplier))
            {
                await ChangeMultiplier(newMultiplier);
                Model.Multiplier = newMultiplier;
                await _dataStore.UpdateAsync(Model);
            }
        }

        async Task ChangeMultiplier(decimal newMultiplier)
        {
            var useFractions = _essentials.GetBoolSetting(AppConstants.MultiplierResultUseFractions);

            Multiplier = newMultiplier;

            if (string.IsNullOrEmpty(Item.Ingredients))
                return;

            Steps[0].SubTitle = Helpers.ApplyMultiplier(Item.Ingredients, Multiplier, useFractions, Resources.IngredientQuantityRegex);

            await TrackEvent("Multiplier", ("Value", newMultiplier.ToString(CultureInfo.CurrentCulture)));
        }

        async Task ShowMoreOptions()
        {
            var selectedOption = Device.RuntimePlatform == Device.Android
                               ? await DisplayActionSheetAsync(Resources.ItemDetailView_MoreTitle, Resources.ItemDetailView_MoreCancel, null,
                                    Resources.ItemDetailView_Share, Resources.ItemDetailView_ShareFile, Resources.ItemDetailView_MoreDelete)
                               : await DisplayActionSheetAsync(Resources.ItemDetailView_MoreTitle, Resources.ItemDetailView_MoreCancel,
                                    Resources.ItemDetailView_MoreDelete, Resources.ItemDetailView_Share, Resources.ItemDetailView_ShareFile);

            if (selectedOption == Resources.ItemDetailView_MoreDelete)
                await DeleteRecipe();
            else if (selectedOption == Resources.ItemDetailView_Share)
                await ShareRecipeText();
            else if (selectedOption == Resources.ItemDetailView_ShareFile)
                await ShareRecipeFile();
        }

        async Task DeleteRecipe()
        {
            var confirmationResult = await DisplayAlertAsync(Resources.ItemDetailView_MoreDelete, Resources.ItemDetailView_MoreDeleteConfirmation,
                    Resources.ItemDetailView_MoreDeleteYes, Resources.ItemDetailView_MoreDeleteNo);

            if (confirmationResult)
            {
                await _dataStore.DeleteAsync(Model);
                await DisplayToastAsync(Resources.ItemDetailView_RecipeDeleted);
                await TrackEvent("DeleteRecipe");
                MessagingCenter.Send<ItemDetailViewModel>(this, "RecipeDeleted");
                await GoBackAsync();
            }
        }

        async Task ShareRecipeText()
        {
            var text = $@"{Item.Title}

{Resources.EditItemView_Ingredients}:
{Item.Ingredients}

{Resources.EditItemView_Instructions}:
{Item.Instructions}";

            await _essentials.ShareText(text, Resources.AppName);
            await TrackEvent("Share");
        }

        async Task ShareRecipeFile()
        {
            try
            {
                using (DisplayLoading(Resources.SettingsView_CreatingBackup))
                {
                    var packagePath = await _recipePackager.PackageRecipes(new[] { Model });
                    await _essentials.ShareFile(packagePath, Resources.AppName);
                    await TrackEvent("ShareFile");
                }
            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }
        }

        async Task ChangeStartTime()
        {
            TimeSpan? result = await DateTimePopups.PickTimeAsync();

            if (result == null)
                return;

            var startTime = DateTime.Today.AddMinutes(result.Value.TotalMinutes);

            PrepareRecipeToDisplay(Item, startTime);
            await TrackEvent("ChangeStartTime");
        }

        async Task ToggleKeepScreenOn()
        {
            KeepScreenOn = !KeepScreenOn;
            _essentials.KeepScreenOn(KeepScreenOn);

            if (KeepScreenOn)
                await this.DisplayToastAsync(Resources.ItemDetailView_ToggleScreenOn);
            else
                await this.DisplayToastAsync(Resources.ItemDetailView_ToggleScreenOff);
        }

        async Task ActivateCookingMode()
        {
            await GoToAsync("items/cook", new Dictionary<string, object> { { "id", Item.Id } });

            //Action<bool, string> action = (bool success, string term) => Debug.WriteLine($"Success: {success}; Spoken: {term}");
            //await _speechRecognizer.RequestAccess();
            //var disposer = _speechRecognizer.ContinuousDictation(action);
        }
    }
}
