using System;
using System.Diagnostics;
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

        public RecipeViewModel Item { get; set; }
        public Recipe Model { get; set; }

        public string Id { get; set; }

        public CookModeViewModel(IDataStore dataStore, IEssentials essentials, ISpeechRecognizer speechRecognizer)
        {
            _dataStore = dataStore;
            _essentials = essentials;
            _speechRecognizer = speechRecognizer;

            Title = "Cook mode";
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
                Item = RecipeViewModel.FromModel(recipe);

                //if (Multiplier != 1)
                //    await ChangeMultiplier(recipe.Multiplier);

                await _speechRecognizer.RequestAccess();
                var disposer = _speechRecognizer.ContinuousDictation(ProcessDictation);

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

        private void ProcessDictation(bool success, string term)
        {

        }
    }
}
