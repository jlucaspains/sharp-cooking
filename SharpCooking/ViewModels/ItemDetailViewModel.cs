using System;
using System.Threading.Tasks;
using SharpCooking.Models;
using SharpCooking.Data;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    [QueryProperty("Id", "id")]
    public class ItemDetailViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;

        public Recipe Item { get; set; }

        public string Id { get; set; }

        public ItemDetailViewModel(IDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public override async Task InitializeAsync()
        {
            if(!int.TryParse(Id, out int parsedId))
            {
                await ReportError("Failed to parse input id");
            }

            Item = await _dataStore.FirstOrDefaultAsync<Recipe>(item => item.Id == parsedId);
            Title = Item?.Title;
        }
    }
}
