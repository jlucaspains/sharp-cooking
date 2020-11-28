using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.ViewModels;

namespace SharpCooking.Services
{
    public interface IRecipePackager
    {
        Task<string> PackageRecipes(IEnumerable<Recipe> recipes);
        Task<(bool Succeeded, string Error)> RestoreRecipePackage(Stream data, bool replaceAll);
        Task<(IEnumerable<Recipe> Recipes, string Error)> GetShareDetail(string filePath = "import.zip");
        Task<(bool Succeded, string Error)> ImportShareFile(string filePath = "import.zip");
        Task CleanupShareFile(string filePath = "import.zip");
        Task CreateDefaultRecipe();
    }

    public class RecipePackager : IRecipePackager
    {
        private readonly IFileHelper _fileHelper;
        private readonly IDataStore _store;

        public RecipePackager(IFileHelper fileHelper, IDataStore dataStore)
        {
            _fileHelper = fileHelper;
            _store = dataStore;
        }

        public async Task<(IEnumerable<Recipe> Recipes, string Error)> GetShareDetail(string filePath = "import.zip")
        {
            using var stream = _fileHelper.ReadStream(filePath);

            using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                if (!zip.Entries.Any(item => item.Name == AppConstants.BackupRecipeFileName))
                    return (null, Resources.SettingsView_BadBackupFile);

                try
                {
                    var entry = zip.GetEntry(AppConstants.BackupRecipeFileName);
                    var jsonStream = entry.Open();

                    using StreamReader reader = new StreamReader(jsonStream);
                    var restoreRecipes = JsonConvert.DeserializeObject<IEnumerable<Recipe>>(await reader.ReadToEndAsync());

                    return (restoreRecipes, null);
                }
                catch
                {
                    return (null, Resources.SettingsView_CorruptedBackupFile);
                }
            }
        }

        public async Task<(bool Succeded, string Error)> ImportShareFile(string filePath = "import.zip")
        {
            using var stream = _fileHelper.ReadStream(filePath);

            var result = await RestoreRecipePackage(stream, false);

            await CleanupShareFile(filePath);

            return result;
        }

        public async Task CreateDefaultRecipe()
        {
            try
            {
                if (await _store.FirstOrDefaultAsync<Recipe>(item => item.Id > 0) != null)
                    return;

                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("SharpCooking.InitialRecipes.zip"))
                {
                    using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        zip.ExtractToDirectory(_fileHelper.GetDocsFolder());

                        var entry = zip.GetEntry(AppConstants.BackupRecipeFileName);
                        var recipeFile = entry.Open();

                        using (StreamReader reader = new StreamReader(recipeFile))
                        {
                            var restoreRecipes = JsonConvert.DeserializeObject<IEnumerable<Recipe>>(await reader.ReadToEndAsync());

                            // add new data
                            foreach (var recipe in restoreRecipes)
                            {
                                recipe.Id = 0;
                                recipe.MainImagePath = recipe.MainImagePath == null
                                    ? null
                                    : Path.GetFileName(recipe.MainImagePath);

                                await _store.InsertAsync(recipe);
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore error
            }
        }

        public async Task<string> PackageRecipes(IEnumerable<Recipe> recipes)
        {
            var allRecipes = recipes.Select(item => item.Clone());

            var allFiles = allRecipes.Where(item => !string.IsNullOrEmpty(item.MainImagePath))
                        .Select(item => _fileHelper.GetFilePath(item.MainImagePath))
                        .ToList();

            // remove the folder path out of the main image path
            foreach (var item in allRecipes)
                item.MainImagePath = Path.GetFileName(item.MainImagePath);

            var recipesFile = _fileHelper.GetFilePath(AppConstants.BackupRecipeFileName);
            var recipesJson = JsonConvert.SerializeObject(allRecipes);
            await _fileHelper.WriteTextAsync(AppConstants.BackupRecipeFileName, recipesJson);

            allFiles.Add(recipesFile);

            var zipPath = _fileHelper.GetFilePath(AppConstants.BackupZipFileName);

            await QuickZip(allFiles.ToArray(), zipPath);

            return zipPath;
        }

        public async Task<(bool Succeeded, string Error)> RestoreRecipePackage(Stream data, bool replaceAll)
        {
            using (ZipArchive zip = new ZipArchive(data, ZipArchiveMode.Read))
            {
                if (!zip.Entries.Any(item => item.Name == AppConstants.BackupRecipeFileName))
                    return (false, Resources.SettingsView_BadBackupFile);

                var appFolder = _fileHelper.GetDocsFolder();
                IEnumerable<Recipe> restoreRecipes = null;

                try
                {
                    var entry = zip.GetEntry(AppConstants.BackupRecipeFileName);
                    var stream = entry.Open();

                    using (StreamReader reader = new StreamReader(stream))
                        restoreRecipes = JsonConvert.DeserializeObject<IEnumerable<Recipe>>(await reader.ReadToEndAsync());
                }
                catch
                {
                    return (false, Resources.SettingsView_CorruptedBackupFile);
                }

                if (restoreRecipes == null || !restoreRecipes.Any())
                    return (false, Resources.SettingsView_NoRecipesToRestore);

                if (replaceAll)
                {
                    var originalFiles = await _fileHelper.GetAllAsync();

                    foreach (var originalFile in originalFiles)
                    {
                        var fileName = Path.GetFileName(originalFile);
                        await _fileHelper.DeleteAsync(fileName);
                    }
                }

                foreach (var recipe in restoreRecipes)
                {
                    if (recipe.MainImagePath != null)
                    {
                        var imagePath = Path.GetFileName(recipe.MainImagePath);

                        var newImageName = $"{Guid.NewGuid().ToString("N")}{Path.GetExtension(imagePath)}";
                        var newImagePath = _fileHelper.GetFilePath(newImageName);

                        zip.GetEntry(imagePath)?.ExtractToFile(newImagePath, true);

                        recipe.MainImagePath = newImageName;
                    }

                    recipe.Id = 0;
                    await _store.InsertAsync(recipe);
                }

                return (true, null);
            }
        }

        public async Task CleanupShareFile(string filePath = "import.zip")
        {
            await _fileHelper.DeleteAsync(filePath);
        }

        async Task<bool> QuickZip(string[] filesToZip, string destinationZipFullPath)
        {
            // Delete existing zip file if exists
            if (await _fileHelper.ExistsAsync(destinationZipFullPath))
                await _fileHelper.DeleteAsync(destinationZipFullPath);

            await Task.Run(() =>
            {
                using (ZipArchive zip = ZipFile.Open(destinationZipFullPath, ZipArchiveMode.Create))
                {
                    foreach (var file in filesToZip)
                    {
                        zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    }
                }
            });

            return await _fileHelper.ExistsAsync(destinationZipFullPath);
        }
    }
}
