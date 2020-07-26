using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public class StepViewModel : BindableModel
    {
        public string Time { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public bool IsNotLast { get; set; }
    }

    public class NoteViewModel : BindableModel
    {
        public int Number { get; set; }
        public string Content { get; set; }
    }

    [QueryProperty("Id", "id")]
    public class ItemDetailViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;
        private readonly IEssentials _essentials;

        public RecipeViewModel Item { get; set; }
        public Recipe Model { get; set; }

        public string Id { get; set; }

        public Command EditCommand { get; }
        public Command ChangeMultiplierCommand { get; }
        public Command MoreCommand { get; }
        public Command ShareRecipeCommand { get; }
        public Command ChangeStartTimeCommand { get; }

        public ObservableCollection<StepViewModel> Steps { get; set; }
        public decimal Multiplier { get; set; }
        public string MultiplierDisplay { get; set; }
        public int StandardStepTimeInterval { get; set; }
        public bool HasNotes { get { return !string.IsNullOrEmpty(Item?.Notes); } }
        public bool DoesNotHaveMainImage { get { return string.IsNullOrEmpty(Item?.MainImagePath); } }
        public bool HasMainImage { get { return !string.IsNullOrEmpty(Item?.MainImagePath); } }

        public ItemDetailViewModel(IDataStore dataStore, IEssentials essentials)
        {
            _dataStore = dataStore;
            _essentials = essentials;

            EditCommand = new Command(async () => await GotoEdit());
            ChangeMultiplierCommand = new Command(async () => await ChangeMultiplier());
            MoreCommand = new Command(async () => await ShowMoreOptions());
            ShareRecipeCommand = new Command(async () => await ShareRecipe());
            ChangeStartTimeCommand = new Command(async () => await ChangeStartTime());
        }

        public override async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;

                if (!int.TryParse(Id, out int parsedId))
                {
                    await ReportError(Resources.ItemDetailView_FailedToParseInputId);
                    return;
                }

                var recipe = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);
                Model = recipe;
                Item = RecipeViewModel.FromModel(recipe);

                PrepareRecipeToDisplay(Item);

                IsBusy = false;

            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }

            await base.InitializeAsync();
        }

        void PrepareRecipeToDisplay(RecipeViewModel recipe, DateTime? proposedStart = null)
        {
            var configInterval = _essentials.GetIntSetting(AppConstants.TimeBetweenStepsInterval);

            Title = recipe?.Title;
            Multiplier = 1;
            StandardStepTimeInterval = configInterval <= 0 ? 5 : configInterval;

            var start = proposedStart ?? DateTime.Now;

            Steps = new ObservableCollection<StepViewModel>
            {
                new StepViewModel { IsNotLast = true, Time = start.ToShortTimeString(), Title = Resources.ItemDetailView_Ingredients, SubTitle = Item.Ingredients },
            };

            start = start.AddMinutes(StandardStepTimeInterval);

            var instructions = Item.Instructions?.Split('\n')?.Where(Item => !string.IsNullOrEmpty(Item))?.Select((item, i) => (i, item))
                ?? new (int i, string item)[] { };

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

                var match = Regex.Match(item, Resources.StepTimeIdentifierRegex);

                if (match?.Success ?? false)
                {
                    int.TryParse(match.Groups["Minutes"]?.Value, out var minutes);
                    int.TryParse(match.Groups["Hours"]?.Value, out var hours);
                    int.TryParse(match.Groups["Days"]?.Value, out var days);

                    start = start.AddMinutes(minutes);
                    start = start.AddHours(hours);
                    start = start.AddDays(days);
                }
                else
                {
                    start = start.AddMinutes(StandardStepTimeInterval);
                }
            }

            Steps.Add(new StepViewModel { IsNotLast = false, Title = Resources.ItemDetailView_Enjoy, Time = getDisplayTime(start, previous) });
        }

        async Task GotoEdit()
        {
            await GoToAsync("items/edit", new Dictionary<string, object> { { "id", Item.Id } });
        }

        async Task ChangeMultiplier()
        {
            var useFractions = _essentials.GetBoolSetting(AppConstants.MultiplierResultUseFractions);

            var inputMultiplier = await DisplayPromptAsync(Resources.ItemDetailView_MultiplierTitle, Resources.ItemDetailView_MultiplierDescription,
                Resources.ItemDetailView_MultiplierOk, Resources.ItemDetailView_MultiplierCancel, Multiplier.ToString(), Keyboard.Numeric);

            if (decimal.TryParse(inputMultiplier, NumberStyles.Float, CultureInfo.CurrentCulture, out var newMultiplier))
            {
                Multiplier = newMultiplier;

                var regexResult = Regex.Replace(Item.Ingredients, Resources.IngredientQuantityRegex, (match) =>
                {
                    var fractionGroup = match.Groups["Fraction"];
                    var regularGroup = match.Groups["Regular"];
                    decimal parsedMatch = 0;

                    if(fractionGroup.Success)
                    {
                        var parts = fractionGroup.Value.Split('/');
                        decimal.TryParse(parts[0], out var fracNumerator);
                        decimal.TryParse(parts[1], out var fracDecimal);

                        if (fracNumerator == 0 || fracDecimal == 0)
                            return "0";

                        parsedMatch = fracNumerator / fracDecimal;
                    }
                    else
                    {
                        decimal.TryParse(regularGroup.Value, out parsedMatch);
                    }

                    var newIngredientValue = parsedMatch * Multiplier;

                    if (!useFractions)
                        return newIngredientValue.ToString("G29");

                    var whole = decimal.Floor(newIngredientValue);

                    if (whole == newIngredientValue)
                    {
                        return newIngredientValue.ToString("0");
                    }
                    else
                    {
                        (var numerator, var denominator) = Fraction.Get(newIngredientValue - whole);
                        return whole == 0 ? $"{numerator}/{denominator}" : $"{whole:0} {Resources.MultiplierQuantityAggregator} {numerator}/{denominator}";
                    }
                }, RegexOptions.Multiline);

                Steps[0].SubTitle = regexResult;

                await TrackEvent("Multiplier", ("Value", newMultiplier.ToString()));
            }
        }

        async Task ShowMoreOptions()
        {
            var selectedOption = Device.RuntimePlatform == Device.Android
                               ? await DisplayActionSheetAsync(Resources.ItemDetailView_MoreTitle, Resources.ItemDetailView_MoreCancel, null,
                                    Resources.ItemDetailView_Share, Resources.ItemDetailView_MoreDelete)
                               : await DisplayActionSheetAsync(Resources.ItemDetailView_MoreTitle, Resources.ItemDetailView_MoreCancel,
                                    Resources.ItemDetailView_MoreDelete, Resources.ItemDetailView_Share);

            if (selectedOption == Resources.ItemDetailView_MoreDelete)
                await DeleteRecipe();
            else if (selectedOption == Resources.ItemDetailView_Share)
                await ShareRecipe();
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
                await GoBackAsync();
            }
        }

        async Task ShareRecipe()
        {
            var text = $@"{Item.Title}

{Resources.EditItemView_Ingredients}:
{Item.Ingredients}

{Resources.EditItemView_Instructions}:
{Item.Instructions}";

            await _essentials.ShareText(text, Resources.AppName);
            await TrackEvent("Share");
        }

        async Task ChangeStartTime()
        {
            var result = await DisplayTimePromptAsync(Resources.ItemDetailView_StartTimeTitle, Resources.ItemDetailView_StartTimeOk, Resources.ItemDetailView_StartTimeCancel);

            if (result == null)
                return;

            var startTime = DateTime.Today.AddMinutes(result.Value.TotalMinutes);

            PrepareRecipeToDisplay(Item, startTime);
            await TrackEvent("ChangeStartTime");
        }
    }
}
