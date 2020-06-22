using Xamarin.Forms;
using SharpCooking.Models;
using SharpCooking.Data;

namespace SharpCooking.Views
{
    public class RecipeSearchHandler : SearchHandler
    {
        private readonly IDataStore _dataStore;

        public RecipeSearchHandler()
        {
            _dataStore = TinyIoC.TinyIoCContainer.Current.Resolve<IDataStore>();
        }
        
        protected override async void OnQueryChanged(string oldValue, string newValue)
        {
            //base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = new System.Collections.Generic.List<Recipe>();
            }
            else
            {
                ItemsSource = await _dataStore.QueryAsync<Recipe>(item => item.Title.ToLower().Contains(newValue.ToLower()));
            }
        }

        protected override async void OnItemSelected(object item)
        {
            base.OnItemSelected(item);

            await (App.Current.MainPage as Xamarin.Forms.Shell).GoToAsync($"items/detail?id={((Recipe)item).Id}");
        }
    }
}
