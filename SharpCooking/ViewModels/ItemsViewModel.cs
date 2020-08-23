using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using SharpCooking.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public sealed class ItemsViewModel : BaseViewModel, IDisposable
    {
        private readonly IDataStore _dataStore;
        private readonly IEssentials _essentials;
        private CancellationTokenSource _throttleCts = new CancellationTokenSource();
        private bool _releaseNotesShown;

        public ObservableCollection<RecipeViewModel> Items { get; } = new ObservableCollection<RecipeViewModel>();
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command ItemTappedCommand { get; }
        public Command FilterListCommand { get; }
        public bool IsRefreshing { get; set; }
        public bool NoDataToShow { get; set; }
        public bool DataToShow { get { return !NoDataToShow; } }
        public string SearchValue { get; set; }

        public ItemsViewModel(IDataStore dataStore, IEssentials essentials)
        {
            _dataStore = dataStore;
            _essentials = essentials;
            Title = Resources.AllRecipes;
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

            if (!_releaseNotesShown && _essentials.IsFirstLaunchForCurrentBuild())
            {
                _releaseNotesShown = true;
                var viewReleaseNotes = await DisplayAlertAsync($"{Resources.ItemsView_WelcomeToVersion} {_essentials.GetVersion()}", 
                    Resources.ItemsView_ViewReleaseNotes, Resources.ItemsView_Yes, Resources.ItemsView_No);

                if (viewReleaseNotes)
                    await GoToAsync("about");
            }

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
                    : await _dataStore.QueryAsync<Recipe>(item => item.Title.ToLower(CultureInfo.CurrentCulture).Contains(SearchValue.ToLower(CultureInfo.CurrentCulture)));

                var sortedItems = items.OrderBy(item => item.Title).ToList();

                foreach (var item in sortedItems)
                    Items.Add(RecipeViewModel.FromModel(item));

                NoDataToShow = !Items.Any();

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

        public void Dispose()
        {
            _throttleCts.Dispose();
        }
    }
}