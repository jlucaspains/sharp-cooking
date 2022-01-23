using System.Threading.Tasks;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    [QueryProperty("Id", "id")]
    public class ItemDetailImageGaleryViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;

        public ItemDetailImageGaleryViewModel(IDataStore dataStore)
        {
            Title = Resources.ItemDetailImageGaleryView_Title;
            _dataStore = dataStore;
        }

        public override async Task InitializeAsync()
        {
            if (!int.TryParse(Id, out int parsedId))
            {
                await ReportError(Resources.ItemDetailView_FailedToParseInputId);
                return;
            }

            var recipe = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);
            var recipeModel = RecipeViewModel.FromModel(recipe);

            ImagePath = recipeModel.MainImagePath;

            await base.InitializeAsync();
        }

        public string Id { get; set; }
        public string ImagePath { get; set; }
    }
}