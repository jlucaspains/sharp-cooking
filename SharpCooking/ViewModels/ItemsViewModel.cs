using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using SharpCooking.Views;
using System.Collections.Generic;
using SharpCooking.Models;
using SharpCooking.Data;
using SharpCooking.Localization;

namespace SharpCooking.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;
        public ObservableCollection<Recipe> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command AddItemCommand { get; private set; }
        public Command ItemTappedCommand { get; private set; }

        public ItemsViewModel(IDataStore dataStore)
        {
            _dataStore = dataStore;
            Title = Resources.AllRecipes;
            Items = new ObservableCollection<Recipe>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            AddItemCommand = new Command(async () => await AddItem());
            ItemTappedCommand = new Command<Recipe>(async (item) => await GoToItemDetail(item));

            MessagingCenter.Subscribe<EditItemView, Recipe>(this, "AddItem", (obj, item) =>
            {
                var newItem = item as Recipe;
                Items.Add(newItem);
                //await _dataStore.AddItemAsync(newItem);
            });
        }

        public override async Task InitializeAsync()
        {
            await ExecuteLoadItemsCommand();
        }

        async Task AddItem()
        {
            await GoToAsync("items/new");
        }

        async Task GoToItemDetail(Recipe item)
        {
            await GoToAsync("items/detail", new Dictionary<string, object> { { "id", item.Id } });
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await _dataStore.AllAsync<Recipe>();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}