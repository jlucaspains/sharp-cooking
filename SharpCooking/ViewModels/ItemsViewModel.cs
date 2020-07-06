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
using System.Threading;

namespace SharpCooking.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private readonly IDataStore _dataStore;
        private CancellationTokenSource _throttleCts = new CancellationTokenSource();

        public ObservableCollection<RecipeViewModel> Items { get; set; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command ItemTappedCommand { get; }
        public Command FilterListCommand { get; }
        public bool IsRefreshing { get; set; }
        public string SearchValue { get; set; }

        public ItemsViewModel(IDataStore dataStore)
        {
            _dataStore = dataStore;
            Title = Resources.AllRecipes;
            Items = new ObservableCollection<RecipeViewModel>();
            LoadItemsCommand = new Command(async () => await Refresh());
            AddItemCommand = new Command(async () => await AddItem());
            ItemTappedCommand = new Command<RecipeViewModel>(async (item) => await GoToItemDetail(item));
            FilterListCommand = new Command(async () => await DebouncedSearch());

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

                var items = string.IsNullOrEmpty(SearchValue)
                    ? await _dataStore.AllAsync<Recipe>()
                    : await _dataStore.QueryAsync<Recipe>(item => item.Title.ToLower().Contains(SearchValue.ToLower()));

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

        private async Task DebouncedSearch()
        {
            try
            {
                Interlocked.Exchange(ref _throttleCts, new CancellationTokenSource()).Cancel();

                await Task.Delay(TimeSpan.FromMilliseconds(500), _throttleCts.Token)
                    .ContinueWith(async task => await Refresh(),
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch
            {
                //Ignore any Threading errors
            }
        }
    }
}