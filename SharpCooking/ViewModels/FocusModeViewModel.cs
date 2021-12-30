using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    [QueryProperty("Id", "id")]
    public sealed class FocusModeViewModel : BaseViewModel, IDisposable
    {
        private readonly IDataStore _dataStore;
        private readonly IEssentials _essentials;
        private readonly ISpeechRecognizer _speechRecognizer;
        private readonly INotificationService _notificationService;
        private Action _speechRecognizerDisposer;
        private CancellationTokenSource _cancellationTokenSource;

        //public RecipeViewModel Item { get; set; }
        public Recipe Model { get; set; }

        public string Id { get; set; }
        public IEnumerable<FocusModeStepViewModel> Steps { get; set; }
        public int Position { get; set; }
        public bool IsMicrophoneDisabled { get; set; } = true;
        public bool IsNarrationDisabled { get; set; } = true;
        public Command CurrentItemChanged { get; }
        public Command MoreOptionsCommand { get; }
        public Command StartTimerCommand { get; }
        public Command StopTimerCommand { get; }
        public Command RestartTimerCommand { get; }

        public FocusModeViewModel(IDataStore dataStore, IEssentials essentials, ISpeechRecognizer speechRecognizer, INotificationService notificationService)
        {
            _dataStore = dataStore;
            _essentials = essentials;
            _speechRecognizer = speechRecognizer;
            _notificationService = notificationService;

            Title = Resources.FocusModeView_Title;

            CurrentItemChanged = new Command(async () => await StepChanged());
            MoreOptionsCommand = new Command(async () => await MoreOptions());
            StartTimerCommand = new Command(StartStepTimer);
            StopTimerCommand = new Command(StopStepTimer);
            RestartTimerCommand = new Command(RestartStepTimer);
        }

        public override async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                _essentials.KeepScreenOn(true);

                IsNarrationDisabled = !_essentials.GetBoolSetting(AppConstants.FocusModeIsNarrationEnabled);

                if (!int.TryParse(Id, out int parsedId))
                {
                    await ReportError(Resources.ItemDetailView_FailedToParseInputId);
                    return;
                }

                if (Model?.Id == parsedId)
                    return;

                var recipe = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);
                Model = recipe;

                PrepareRecipeToDisplay(Model);

                if (Position < 0)
                    Position = 0;

                await _speechRecognizer.RequestAccess();
            }
            catch (Exception ex)
            {
                await TrackException(ex);
                await DisplayAlertAsync(Resources.ErrorTitle, Resources.EditItemView_UnknownError, Resources.ErrorOk);
            }
            finally
            {
                IsBusy = false;
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

        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void PrepareRecipeToDisplay(Recipe model)
        {
            var useFractions = _essentials.GetBoolSetting(AppConstants.MultiplierResultUseFractions);
            var ingredients = Helpers.ApplyMultiplier(model.Ingredients, model.Multiplier, useFractions, Resources.IngredientQuantityRegex);

            var start = new[]
            {
                new FocusModeStepViewModel { SubTitle = model.Title },
                new FocusModeStepViewModel { SubTitle = Resources.FocusModeView_Ingredients }
            };
            var ingredientSteps = ingredients.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select((item, index) => new FocusModeStepViewModel
                {
                    Title = $"{Resources.FocusModeView_Ingredient} {index + 1}",
                    SubTitle = item
                }).ToArray();

            var middle = new[]
            {
                new FocusModeStepViewModel { SubTitle = Resources.FocusModeView_StepByStep }
            };

            var instructions = model.Instructions.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select((item, index) => new FocusModeStepViewModel
                {
                    Title = $"{Resources.FocusModeView_Step} {index + 1}",
                    SubTitle = item,
                    Time = Helpers.GetImpliedTimeFromString(item, Resources.StepTimeIdentifierRegex)
                })
                .Select(item =>
                {
                    item.HasTime = !TimeSpan.Zero.Equals(item.Time);
                    item.OriginalTime = item.Time;

                    return item;
                }).ToArray();

            Steps = start.Concat(ingredientSteps).Concat(middle).Concat(instructions).ToArray();
        }

        private async Task ProcessDictation(bool success, string term)
        {
            if (!success || string.IsNullOrEmpty(term))
                return; // Need to do something on failure

            if (term.Equals(Resources.FocusModeView_Next, StringComparison.CurrentCultureIgnoreCase) && Position < (Steps.Count() - 1))
            {
                Position++;
            }
            else if (term.Equals(Resources.FocusModeView_Previous, StringComparison.CurrentCultureIgnoreCase) && Position > 0)
            {
                Position--;
            }
            else if (term.Equals(Resources.FocusModeView_Stop, StringComparison.CurrentCultureIgnoreCase))
            {
                await GoBackAsync();
            }
            else if (term.Equals(Resources.FocusModeView_StartTimer, StringComparison.CurrentCultureIgnoreCase))
            {
                StartStepTimer();
            }
            else if (term.Equals(Resources.FocusModeView_StopTimer, StringComparison.CurrentCultureIgnoreCase))
            {
                StopStepTimer();
            }
        }

        private async Task StepChanged()
        {
            try
            {
                if (_speechRecognizerDisposer != null)
                {
                    _speechRecognizerDisposer();
                    _speechRecognizerDisposer = null;
                }

                if (Position < 0 || string.IsNullOrEmpty(Steps.ElementAt(Position)?.SubTitle))
                    return;

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                if (!IsNarrationDisabled)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    await _essentials.SpeakAsync(Steps.ElementAt(Position).SubTitle, _cancellationTokenSource.Token);
                }

                if (!IsMicrophoneDisabled)
                    _speechRecognizerDisposer = _speechRecognizer.ContinuousDictation(async (success, term) => await ProcessDictation(success, term));
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to process next step. {ex}", "OK");
            }
        }

        private void StartStepTimer()
        {
            var currentStep = Steps.ElementAt(Position);

            if (currentStep.HasTime && !currentStep.IsRunning)
            {
                var notificationTitle = Resources.FocusModeView_TimerNotificationTitle;
                var notificationMessage = string.Format(CultureInfo.CurrentCulture, Resources.FocusModeView_StepTimerIsDone, Position);
                currentStep.NotificationId = _notificationService.SendNotification(notificationTitle, notificationMessage, DateTime.Now.Add(currentStep.Time));

                var oneSecond = TimeSpan.FromSeconds(1);
                Device.StartTimer(oneSecond, () =>
                {
                    var shouldContinue = currentStep.Time > TimeSpan.Zero && currentStep.IsRunning;

                    if (shouldContinue)
                    {
                        currentStep.Time = currentStep.Time.Subtract(oneSecond);

                        if (currentStep.Time == TimeSpan.Zero)
                            currentStep.IsRunning = false;
                    }

                    return shouldContinue;
                });
                currentStep.IsRunning = true;
            }
        }

        private void StopStepTimer()
        {
            var currentStep = Steps.ElementAt(Position);
            currentStep.IsRunning = false;

            if (currentStep.NotificationId > 0)
            {
                _notificationService.DeleteNotification(currentStep.NotificationId);
                currentStep.NotificationId = 0;
            }
        }

        private void RestartStepTimer()
        {
            StopStepTimer();

            var currentStep = Steps.ElementAt(Position);
            currentStep.Time = currentStep.OriginalTime;

            if (!currentStep.IsRunning)
                StartStepTimer();
        }

        private void SetNarrationStatus(bool enabled)
        {
            IsNarrationDisabled = !enabled;

            _essentials.SetBoolSetting(AppConstants.FocusModeIsNarrationEnabled, enabled);
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

            //options.Add(IsMicrophoneDisabled ? Resources.FocusModeView_EnableMicrophone : Resources.FocusModeView_DisableMicrophone);
            options.Add(IsNarrationDisabled ? Resources.FocusModeView_EnableNarration : Resources.FocusModeView_DisableNarration);

            var result = await DisplayActionSheetAsync(Resources.FocusModelView_MoreTitle, Resources.FocusModelView_MoreCancel, null, options.ToArray());

            if (result == Resources.FocusModeView_EnableMicrophone)
            {
                SetMicrophoneStatus(true);
            }
            else if (result == Resources.FocusModeView_EnableNarration)
            {
                SetNarrationStatus(true);
            }
            else if (result == Resources.FocusModeView_DisableMicrophone)
            {
                SetMicrophoneStatus(false);
            }
            else if (result == Resources.FocusModeView_DisableNarration)
            {
                SetNarrationStatus(false);
            }
        }
    }
}
