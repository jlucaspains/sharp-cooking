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
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                ItemsSource = await _dataStore.QueryAsync<Recipe>(item => item.Title.Contains(newValue));
            }
        }

        protected override async void OnItemSelected(object item)
        {
            base.OnItemSelected(item);

            // Note: strings will be URL encoded for navigation (e.g. "Blue Monkey" becomes "Blue%20Monkey"). Therefore, decode at the receiver.
            await (App.Current.MainPage as Xamarin.Forms.Shell).GoToAsync($"items/detail?id={((Recipe)item).Id}");
        }
    }
}
