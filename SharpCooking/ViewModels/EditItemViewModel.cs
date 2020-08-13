using HtmlAgilityPack;
using Newtonsoft.Json;
using Plugin.Media.Abstractions;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    [QueryProperty("Id", "id")]
    public class EditItemViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;
        private readonly IEssentials _essentials;
        private readonly IFileHelper _fileHelper;

        public EditItemViewModel(IDataStore dataStore, IEssentials essentials, IFileHelper fileHelper)
        {
            Item = new RecipeViewModel();

            SaveCommand = new Command(async () => await Save());
            MainImageTappedCommand = new Command(async () => await PickImage());
            ImportCommand = new Command(async () => await ImportFromWebsite());

            _dataStore = dataStore;
            _essentials = essentials;
            _fileHelper = fileHelper;
        }

        public RecipeViewModel Item { get; set; }
        public Command SaveCommand { get; set; }
        public Command MainImageTappedCommand { get; set; }
        public Command ImportCommand { get; }
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
                await DisplayAlertAsync(Resources.EditItemView_SomethingWrong, Resources.EditItemView_UnknownError, Resources.ErrorOk);
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
                await DisplayAlertAsync(Resources.EditItemView_SomethingWrong, Resources.EditItemView_UnknownError, Resources.ErrorOk);
                await TrackException(ex);
            }
        }

        private async Task PickImage()
        {
            try
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

                if (!await _fileHelper.ExistsAsync(fileName))
                    await _fileHelper.CopyAsync(result.Path, fileName);

                Item.MainImagePath = _fileHelper.GetFilePath(fileName);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(Resources.EditItemView_SomethingWrong, Resources.EditItemView_UnknownError, Resources.ErrorOk);
                await TrackException(ex);
            }
        }

        private async Task ImportFromWebsite()
        {
            if (!string.IsNullOrEmpty(Id))
                return;

            var url = await DisplayPromptAsync(Resources.EditItemView_ImportRecipe, Resources.EditItemView_CopyUrl, Resources.EditItemView_ImportOk, Resources.EditItemView_ImportCancel);

            if (string.IsNullOrEmpty(url))
            {
                await DisplayAlertAsync(Resources.EditItemView_SomethingWrong, Resources.EditItemView_AddressWrong, Resources.ErrorOk);
                return;
            }

            await TrackEvent("ImportFromWebsite");

            using (DisplayLoading(Resources.EditItemView_DownloadingRecipe))
            {
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        var latestConfiguration = await TryGetImportConfig(AppConstants.ImportConfigurationFileName, httpClient);

                        var uri = new Uri(url);
                        var hostName = uri.Host;
                        var config = latestConfiguration.FirstOrDefault(x => x.Hostname == hostName);

                        if (config == null)
                        {
                            await DisplayAlertAsync(Resources.EditItemView_SomethingWrong, Resources.EditItemView_AddressWrong, Resources.ErrorOk);
                            return;
                        }

                        Item = new RecipeViewModel();
                        var web = new HtmlWeb();

                        var htmlDoc = await web.LoadFromWebAsync(url);

                        var title = await TryGetTitle(htmlDoc, config);
                        var imageName = await TryGetImagePath(htmlDoc, config);
                        var ingredients = await TryGetIngredients(htmlDoc, config);
                        var steps = await TryGetSteps(htmlDoc, config);

                        Item = new RecipeViewModel
                        {
                            Title = title,
                            Ingredients = string.Join("\r\n", ingredients),
                            Instructions = string.Join("\r\n\r\n", steps),
                            Source = hostName,
                            MainImagePath = _fileHelper.GetFilePath(imageName),
                            Notes = $"{Resources.EditItemView_Source}: {uri}"
                        };
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync(Resources.EditItemView_SomethingWrong, Resources.EditItemView_ImportFailed, Resources.ErrorOk);
                        await TrackException(ex);
                    }
                }
            }
        }

        private string TryGetNameFromContentType(string contentType)
        {
            var guid = Guid.NewGuid();

            switch (contentType)
            {
                case "image/jpeg":
                    return $"{guid}.jpeg";
                case "image/png":
                    return $"{guid}.png";
                case "image/bmp":
                    return $"{guid}.bmp";
                case "image/gif":
                    return $"{guid}.gif";
                default:
                    return null;
            }
        }

        private Task<string> TryGetTitle(HtmlDocument htmlDoc, RecipeDownloadConfig config)
        {
            foreach (var titleXPath in config.TitleXPath)
            {
                var titleNode = htmlDoc?.DocumentNode?.SelectSingleNode(titleXPath);

                if (titleNode != null)
                    return Task.FromResult(titleNode?.InnerText?.Trim());
            }

            return Task.FromResult((string)null);
        }

        private async Task<string> TryGetImagePath(HtmlDocument htmlDoc, RecipeDownloadConfig config)
        {
            foreach (var imageXPath in config.ImageXPath)
            {
                var imageNode = htmlDoc.DocumentNode.SelectSingleNode(imageXPath);
                var image = imageNode?.GetAttributeValue<string>(config.ImageAttribute, null);

                if (string.IsNullOrEmpty(image))
                    continue;

                string fileName = null;

                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        var response = await httpClient.GetAsync(image);

                        if (!response.IsSuccessStatusCode)
                            continue;

                        var imageContent = await response.Content.ReadAsStreamAsync();
                        var contentTypes = response.Content.Headers.GetValues("content-type");
                        var contentType = contentTypes.FirstOrDefault();

                        if (!string.IsNullOrEmpty(contentType))
                        {
                            fileName = TryGetNameFromContentType(contentType);

                            await _fileHelper.WriteStreamAsync(fileName, imageContent);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(fileName))
                    return fileName;
            }

            return null;
        }

        private Task<IEnumerable<string>> TryGetIngredients(HtmlDocument htmlDoc, RecipeDownloadConfig config)
        {
            foreach (var ingredientsXPath in config.IngredientsXPath)
            {
                var ingredientNodes = htmlDoc?.DocumentNode?.SelectNodes(ingredientsXPath);
                var result = ingredientNodes?.Select(x => SanitizeHtml(HttpUtility.HtmlDecode(x.InnerText.Trim())));

                if (result != null && result.Any())
                    return Task.FromResult(result);
            }

            return Task.FromResult((IEnumerable<string>)new string[] { });
        }

        private Task<IEnumerable<string>> TryGetSteps(HtmlDocument htmlDoc, RecipeDownloadConfig config)
        {
            foreach (var preparationXPath in config.PreparationXPath)
            {
                var stepNodes = htmlDoc?.DocumentNode?.SelectNodes(preparationXPath);
                var result = stepNodes?.Select(x => SanitizeHtml(HttpUtility.HtmlDecode(x.InnerText.Trim())));

                if (result != null && result.Any())
                    return Task.FromResult(result);
            }

            return Task.FromResult((IEnumerable<string>)new string[] { });
        }

        private async Task<RecipeDownloadConfig[]> TryGetImportConfig(string configFileName, HttpClient client)
        {
            var defaultResult = new RecipeDownloadConfig[] { };

            try
            {
                var localConfigVersion = _essentials.GetStringSetting(AppConstants.RecipeDownloadConfigVersionSetting) ?? string.Empty;
                var remoteConfigVersion = await TryGetRemoteImportConfigVersion(client);
                string config = null;

                if (!localConfigVersion.Equals(remoteConfigVersion, StringComparison.OrdinalIgnoreCase))
                {
                    config = await TryDownloadImportConfigFile(configFileName, client);
                    _essentials.SetStringSetting(AppConstants.RecipeDownloadConfigVersionSetting, remoteConfigVersion);
                }

                if (string.IsNullOrEmpty(config) && await _fileHelper.ExistsAsync(configFileName))
                    config = await _fileHelper.ReadTextAsync(configFileName);

                if (string.IsNullOrEmpty(config))
                    return defaultResult;

                return JsonConvert.DeserializeObject<RecipeDownloadConfig[]>(config);
            }
            catch (Exception ex)
            {
                await TrackException(ex);

                return defaultResult;
            }
        }

        private async Task<string> TryGetRemoteImportConfigVersion(HttpClient client)
        {
            var response = await client.GetAsync(AppConstants.RecipeDownloadConfigVersionUrl);

            if (response.IsSuccessStatusCode)
                return await response.Content?.ReadAsStringAsync() ?? string.Empty;

            return string.Empty;
        }

        private async Task<string> TryDownloadImportConfigFile(string configFileName, HttpClient client)
        {
            var response = await client.GetAsync(AppConstants.RecipeDownloadConfigUrl);

            if (response.IsSuccessStatusCode)
            {
                var config = await response.Content?.ReadAsStringAsync() ?? string.Empty;
                await _fileHelper.WriteTextAsync(configFileName, config);

                return config;
            }

            return null;
        }

        private static string SanitizeHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            var result = input.Replace("½", "1/2")
                .Replace("⅓", "1/3")
                .Replace("⅔", "2/3")
                .Replace("¼", "1/4")
                .Replace("¾", "3/4")
                .Replace("⅕", "1/5")
                .Replace("⅖", "2/5")
                .Replace("⅗", "3/5")
                .Replace("⅘", "4/5")
                .Replace("⅙", "1/6")
                .Replace("⅚", "5/6")
                .Replace("⅐", "1/7")
                .Replace("⅛", "1/8")
                .Replace("⅜", "3/8")
                .Replace("⅝", "5/8")
                .Replace("⅞", "7/8")
                .Replace("⅑", "1/9")
                .Replace("⅒", "1/10");

            return Regex.Replace(result, "\\s+", " ");
        }
    }
}