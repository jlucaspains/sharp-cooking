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
        public ObservableCollection<RecipeViewModel> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command AddItemCommand { get; private set; }
        public Command ItemTappedCommand { get; private set; }
        public bool IsRefreshing { get; set; }

        public ItemsViewModel(IDataStore dataStore)
        {
            _dataStore = dataStore;
            Title = Resources.AllRecipes;
            Items = new ObservableCollection<RecipeViewModel>();
            LoadItemsCommand = new Command(async () => await Refresh());
            AddItemCommand = new Command(async () => await AddItem());
            ItemTappedCommand = new Command<RecipeViewModel>(async (item) => await GoToItemDetail(item));

            MessagingCenter.Subscribe<EditItemView, Recipe>(this, "AddItem", (obj, item) =>
            {
                var newItem = item as Recipe;
                Items.Add(RecipeViewModel.FromModel(newItem));
            });
        }

        public override async Task InitializeAsync()
        {
            await Refresh();

            await base.InitializeAsync();
        }

        async Task AddItem()
        {
            await GoToAsync("items/new");
        }

        async Task GoToItemDetail(RecipeViewModel item)
        {
            await GoToAsync("items/detail", new Dictionary<string, object> { { "id", item.Id } });
        }

        async Task Refresh()
        {
            IsRefreshing = true;

            try
            {
                Items.Clear();
                var items = await _dataStore.AllAsync<Recipe>();

                foreach (var item in items)
                    Items.Add(RecipeViewModel.FromModel(item));

                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}