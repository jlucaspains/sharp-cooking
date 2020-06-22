using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public Recipe Item { get; set; }

        public string Id { get; set; }

        public Command EditCommand { get; }
        public Command ChangeMultiplierCommand { get; }
        public Command MoreCommand { get; }
        public Command ShareRecipeCommand { get; }

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
        }

        public override async Task InitializeAsync()
        {
            IsBusy = true;

            if (!int.TryParse(Id, out int parsedId))
            {
                await ReportError("Failed to parse input id");
            }

            Item = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);
            Title = Item?.Title;
            Multiplier = 1;
            StandardStepTimeInterval = _essentials.GetIntSetting(AppConstants.TimeBetweenStepsInterval);

            var start = DateTime.Now;

            Steps = new ObservableCollection<StepViewModel>
            {
                new StepViewModel { IsNotLast = true, Time = start.ToShortTimeString(), Title = "Ingredients", SubTitle = Item.Ingredients },
            };

            start = start.AddMinutes(StandardStepTimeInterval);

            var instructions = Item.Instructions?.Split('\n')?.Where(Item => !string.IsNullOrEmpty(Item))?.Select((item, i) => (i, item))
                ?? new (int i, string item)[] { };

            foreach ((int i, string item) in instructions)
            {
                Steps.Add(new StepViewModel { IsNotLast = true, Title = $"Step {i + 1}", SubTitle = item, Time = start.ToShortTimeString() });

                var match = Regex.Match(item, AppConstants.StepTimeIdentifierRegex);

                if (match?.Success ?? false)
                {
                    int.TryParse(match.Groups["Minutes"]?.Value, out var minutes);
                    int.TryParse(match.Groups["Hours"]?.Value, out var hours);
                    int.TryParse(match.Groups["Days"]?.Value, out var days);

                    start = start.AddMinutes(minutes);
                    start = start.AddHours(hours);
                    start = start.AddMinutes(days);
                }
                else
                {
                    start = start.AddMinutes(StandardStepTimeInterval);
                }
            }

            Steps.Add(new StepViewModel { IsNotLast = false, Title = "Enjoy!", Time = start.ToShortTimeString() });

            IsBusy = false;
        }

        async Task GotoEdit()
        {
            await GoToAsync("items/edit", new Dictionary<string, object> { { "id", Item.Id } });
        }

        async Task ChangeMultiplier()
        {
            var inputMultiplier = await DisplayPromptAsync(Resources.ItemDetailView_MultiplierTitle, Resources.ItemDetailView_MultiplierDescription,
                Resources.ItemDetailView_MultiplierOk, Resources.ItemDetailView_MultiplierCancel, Multiplier.ToString(), Keyboard.Numeric);

            if (decimal.TryParse(inputMultiplier, out var newMultiplier))
            {
                Multiplier = newMultiplier;

                var regexResult = Regex.Replace(Item.Ingredients, AppConstants.IngredientQuantityRegex, (match) =>
                {
                    decimal.TryParse(match.Value, out var parsedMatch);
                    var newIngredientValue = parsedMatch * Multiplier;
                    var whole = decimal.Floor(newIngredientValue);

                    if (whole == newIngredientValue)
                    {
                        return newIngredientValue.ToString("0");
                    }
                    else
                    {
                        (var numerator, var denominator) = Fraction.Get(newIngredientValue - whole);
                        return whole == 0 ? $"{numerator}/{denominator}" : $"{whole:0} and {numerator}/{denominator}";
                    }
                });

                Steps[0].SubTitle = regexResult;
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
                await _dataStore.DeleteAsync(Item);
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
        }
    }
}
