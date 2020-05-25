using Xamarin.Forms;
using SharpCooking.Models;
using System.Threading.Tasks;
using SharpCooking.Data;
using SharpCooking.Localization;
using Plugin.Media.Abstractions;

namespace SharpCooking.ViewModels
{
    [QueryProperty("Id", "id")]
    public class EditItemViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;

        public EditItemViewModel(IDataStore dataStore)
        {
            Item = new RecipeViewModel();

            SaveCommand = new Command(async () => await Save());
            MainImageTappedCommand = new Command(async () => await PickImage());

            _dataStore = dataStore;
        }

        public RecipeViewModel Item { get; set; }
        public Command SaveCommand { get; set; }
        public Command MainImageTappedCommand { get; set; }
        public bool IsFavoriteVisible { get; set; }
        public string Id { get; set; }

        public async Task Save()
        {
            var model = await RecipeViewModel.ToModel(Item);
            await _dataStore.UpsertAsync(model);
            await GoBackAsync();
        }

        public override async Task InitializeAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                Title = Resources.NewRecipe;
                return;
            }

            if (!int.TryParse(Id, out int parsedId))
                await ReportError("Failed to parse input id");

            Title = Resources.EditRecipe;

            var result = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);

            Item = await RecipeViewModel.FromModel(result);
        }

        private async Task PickImage()
        {
            var actionSheetResult = await DisplayActionSheet("How?", "Cancel", null, "Pick image", "Use Camera");
            MediaFile result;
            if(actionSheetResult == "Pick image")
            {
                result = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    CompressionQuality = 92
                });
            }
            else
            {
                result = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    CompressionQuality = 92,
                    PhotoSize = PhotoSize.Medium,
                    AllowCropping = true
                });
            }

            Item.MainImage = (StreamImageSource)ImageSource.FromStream(() => result.GetStream());
        }

    }
}