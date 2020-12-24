using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public class ImportViewModel : BaseViewModel
    {
        private readonly IRecipePackager _recipePackager;

        public ImportViewModel(IRecipePackager recipePackager)
        {
            _recipePackager = recipePackager;
            ConfirmCommand = new Command(async () => await Confirm());
            CancelCommand = new Command(async () => await Cancel());

            Title = Resources.ImportView_Title;
        }

        public Command ConfirmCommand { get; }
        public Command CancelCommand { get; }
        public ObservableCollection<Recipe> ImportDetails { get; } = new ObservableCollection<Recipe>();

        public override async Task InitializeAsync()
        {
            try
            {
                ImportDetails.Clear();

                var result = await _recipePackager.GetShareDetail();

                if (!string.IsNullOrEmpty(result.Error))
                {
                    await ReportError(result.Error);
                    await GoBackAsync();
                    return;
                }

                foreach (var item in result.Recipes)
                    ImportDetails.Add(item);

                await base.InitializeAsync();
            }
            catch (Exception ex)
            {
                await TrackException(ex);
                await ReportError(Resources.ImportView_FailedToImport);
            }
        }

        async Task Confirm()
        {
            try
            {
                var result = await _recipePackager.ImportShareFile();

                if (!result.Succeded)
                    await ReportError(result.Error);
                else
                    await DisplayToastAsync(Resources.ImportView_ImportSuccessful);

                await TrackEvent("ImportRecipe");

                await GoBackAsync();
            }
            catch (Exception ex)
            {
                await TrackException(ex);
                await ReportError(Resources.ImportView_FailedToImport);
            }
        }

        async Task Cancel()
        {
            await _recipePackager.CleanupShareFile();
            await DisplayToastAsync(Resources.ImportView_ImportCancelled);
            await GoBackAsync();
        }
    }
}
