using Xamarin.Forms;
using SharpCooking.Models;
using SharpCooking.Data;
using System.Linq;

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
                ItemsSource = new System.Collections.Generic.List<RecipeViewModel>();
            }
            else
            {
                var result = await _dataStore.QueryAsync<Recipe>(item => item.Title.ToLower().Contains(newValue.ToLower()));
                ItemsSource = result.Select(item => RecipeViewModel.FromModel(item)).ToList();
            }
        }

        protected override async void OnItemSelected(object item)
        {
            base.OnItemSelected(item);

            await (App.Current.MainPage as Xamarin.Forms.Shell).GoToAsync($"items/detail?id={((RecipeViewModel)item).Id}");
        }
    }
}
