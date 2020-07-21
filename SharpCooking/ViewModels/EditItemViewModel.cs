using Plugin.Media.Abstractions;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

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
        public string Id { get; set; }

        public async Task Save()
        {
            try
            {
                var model = RecipeViewModel.ToModel(Item);

                if (Item.Id == 0)
                    await _dataStore.InsertAsync(model);
                else
                    await _dataStore.UpdateAsync(model);

                await TrackEvent("SaveRecipe", ("Type", Item.Id == 0 ? "new" : "edit"));

                await GoBackAsync();
            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }
        }

        public override async Task InitializeAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                {
                    Title = Resources.NewRecipe;
                    return;
                }

                if (!int.TryParse(Id, out int parsedId))
                {
                    await ReportError(Resources.EditItemView_FailedToParse);
                    return;
                }

                Title = Resources.EditRecipe;

                var result = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);

                Item = RecipeViewModel.FromModel(result);

                await base.InitializeAsync();

            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }
        }

        private async Task PickImage()
        {
            var actionSheetResult = await DisplayActionSheetAsync(Resources.EditItemView_PickImageTitle, Resources.EditItemView_PickImageCancel,
                null, Resources.EditItemView_PickImagePickImage, Resources.EditItemView_PickImageUseCamera);

            MediaFile result = null;
            if (actionSheetResult == Resources.EditItemView_PickImagePickImage)
            {
                result = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    CompressionQuality = 92,
                    SaveMetaData = false
                });
            }
            else if (actionSheetResult == Resources.EditItemView_PickImageUseCamera)
            {
                result = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    CompressionQuality = 92,
                    PhotoSize = PhotoSize.Medium,
                    AllowCropping = true
                });
            }

            if (result == null)
                return;

            var fileName = Path.GetFileName(result.Path);
            var appFolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

            var path = Path.Combine(appFolder, fileName);
            File.Copy(result.Path, path);
            Item.MainImagePath = path;
        }
    }
}