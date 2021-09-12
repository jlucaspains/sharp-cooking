using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    [QueryProperty("Id", "id")]
    public class CookModeViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;
        private readonly IEssentials _essentials;
        private readonly ISpeechRecognizer _speechRecognizer;
        private Action _speechRecognizerDisposer;

        //public RecipeViewModel Item { get; set; }
        public Recipe Model { get; set; }

        public string Id { get; set; }
        public IEnumerable<CookModeStepViewModel> Steps { get; set; }
        public int Position { get; set; }
        public bool IsMicrophoneDisabled { get; set; } = true;
        public bool IsNarrationDisabled { get; set; } = true;
        public Command CurrentItemChanged { get; }
        public Command MoreOptionsCommand { get; }
        public Command StartTimerCommand { get; }
        public Command StopTimerCommand { get; }

        public CookModeViewModel(IDataStore dataStore, IEssentials essentials, ISpeechRecognizer speechRecognizer)
        {
            _dataStore = dataStore;
            _essentials = essentials;
            _speechRecognizer = speechRecognizer;

            Title = Resources.CookModeView_Title;

            CurrentItemChanged = new Command(async () => await StepChanged());
            MoreOptionsCommand = new Command(async () => await MoreOptions());
            StartTimerCommand = new Command(StartStepTimer);
            StopTimerCommand = new Command(StopStepTimer);
        }

        public override async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                _essentials.KeepScreenOn(true);

                if (!int.TryParse(Id, out int parsedId))
                {
                    await ReportError(Resources.ItemDetailView_FailedToParseInputId);
                    return;
                }

                var recipe = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);
                Model = recipe;

                PrepareRecipeToDisplay(Model);

                await _speechRecognizer.RequestAccess();

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

            if (_speechRecognizerDisposer != null)
            {
                _speechRecognizerDisposer();
                _speechRecognizerDisposer = null;
            }

            return Task.CompletedTask;
        }

        private void PrepareRecipeToDisplay(Recipe model)
        {
            var useFractions = _essentials.GetBoolSetting(AppConstants.MultiplierResultUseFractions);
            var ingredients = Helpers.ApplyMultiplier(model.Ingredients, model.Multiplier, useFractions, Resources.IngredientQuantityRegex);

            var start = new[]
            {
                new CookModeStepViewModel { SubTitle = model.Title },
                new CookModeStepViewModel { SubTitle = Resources.CookModeView_Ingredients }
            };
            var ingredientSteps = ingredients.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select((item, index) => new CookModeStepViewModel
                {
                    Title = $"{Resources.CookModeView_Ingredient} {index + 1}",
                    SubTitle = item
                }).ToArray();

            var middle = new[]
            {
                new CookModeStepViewModel { SubTitle = Resources.CookModeView_StepByStep }
            };

            var instructions = model.Instructions.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select((item, index) => new CookModeStepViewModel
                {
                    Title = $"{Resources.CookModeView_Step} {index + 1}",
                    SubTitle = item,
                    Time = Helpers.GetImpliedTimeFromString(item, Resources.StepTimeIdentifierRegex)
                })
                .Select(item =>
                {
                    item.HasTime = !TimeSpan.Zero.Equals(item.Time);

                    return item;
                }).ToArray();

            Steps = start.Concat(ingredientSteps).Concat(middle).Concat(instructions).ToArray();
        }

        private async Task ProcessDictation(bool success, string term)
        {
            if (!success || string.IsNullOrEmpty(term))
                return; // Need to do something on failure

            if (term.Equals(Resources.CookModeView_Next, StringComparison.CurrentCultureIgnoreCase) && Position < (Steps.Count() - 1))
            {
                Position++;
            }
            else if (term.Equals(Resources.CookModeView_Previous, StringComparison.CurrentCultureIgnoreCase) && Position > 0)
            {
                Position--;
            }
            else if (term.Equals(Resources.CookModeView_Stop, StringComparison.CurrentCultureIgnoreCase))
            {
                await GoBackAsync();
            }
            else if (term.Equals(Resources.CookModeView_StartTimer, StringComparison.CurrentCultureIgnoreCase))
            {
                StartStepTimer();
            }
            else if (term.Equals(Resources.CookModeView_StopTimer, StringComparison.CurrentCultureIgnoreCase))
            {
                StopStepTimer();
            }
        }

        private async Task StepChanged()
        {
            if (_speechRecognizerDisposer != null)
            {
                _speechRecognizerDisposer();
                _speechRecognizerDisposer = null;
            }

            if (Position < 0 || string.IsNullOrEmpty(Steps.ElementAt(Position)?.SubTitle))
                return;

            if (!IsNarrationDisabled)
                await _essentials.SpeakAsync(Steps.ElementAt(Position).SubTitle);

            if (!IsMicrophoneDisabled)
                _speechRecognizerDisposer = _speechRecognizer.ContinuousDictation(async (success, term) => await ProcessDictation(success, term));
        }

        private void StartStepTimer()
        {
            var currentStep = Steps.ElementAt(Position);

            if (currentStep.HasTime && !currentStep.IsRunning)
            {
                var oneSecond = TimeSpan.FromSeconds(1);
                Device.StartTimer(oneSecond, () =>
                {
                    var shouldContinue = currentStep.Time > TimeSpan.Zero && currentStep.IsRunning;

                    if(shouldContinue)
                        currentStep.Time = currentStep.Time.Subtract(oneSecond);

                    return shouldContinue;
                });
                currentStep.IsRunning = true;
            }
        }

        private void StopStepTimer()
        {
            var currentStep = Steps.ElementAt(Position);
            currentStep.IsRunning = false;
        }

        private void SetNarrationStatus(bool enabled)
        {
            IsNarrationDisabled = !enabled;
        }

        private void SetMicrophoneStatus(bool enabled)
        {
            IsMicrophoneDisabled = !enabled;

            if (IsMicrophoneDisabled && _speechRecognizerDisposer != null)
            {
                _speechRecognizerDisposer();
                _speechRecognizerDisposer = null;
            }
        }

        private async Task MoreOptions()
        {
            var options = new List<string>();

            options.Add(IsMicrophoneDisabled ? Resources.CookModeView_EnableMicrophone : Resources.CookModeView_DisableMicrophone);
            options.Add(IsNarrationDisabled ? Resources.CookModeView_EnableNarration : Resources.CookModeView_DisableNarration);

            var result = await DisplayActionSheetAsync(Resources.CookModelView_MoreTitle, Resources.CookModelView_MoreCancel, null, options.ToArray());

            if (result == Resources.CookModeView_EnableMicrophone)
            {
                SetMicrophoneStatus(true);
            }
            else if (result == Resources.CookModeView_EnableNarration)
            {
                SetNarrationStatus(true);
            }
            else if (result == Resources.CookModeView_DisableMicrophone)
            {
                SetMicrophoneStatus(false);
            }
            else if (result == Resources.CookModeView_DisableNarration)
            {
                SetNarrationStatus(false);
            }
        }
    }
}
