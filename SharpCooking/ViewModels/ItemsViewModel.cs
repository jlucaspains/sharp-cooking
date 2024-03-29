﻿using Newtonsoft.Json;
using SharpCooking.Data;
using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using SharpCooking.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public sealed class ItemsViewModel : BaseViewModel, IDisposable
    {
        private readonly IDataStore _dataStore;
        private readonly IRecipePackager _recipePackager;
        private readonly IEssentials _essentials;
        private CancellationTokenSource _throttleCts = new CancellationTokenSource();
        private bool _releaseNotesShown;

        public ObservableCollection<RecipeViewModel> Items { get; } = new ObservableCollection<RecipeViewModel>();
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command ItemTappedCommand { get; }
        public Command FilterListCommand { get; }
        public Command SortCommand { get; }
        public bool IsRefreshing { get; set; }
        public bool NoDataToShow { get; set; }
        public bool DataToShow { get { return !NoDataToShow; } }
        public string SearchValue { get; set; }

        public ItemsViewModel(IDataStore dataStore, IRecipePackager recipePackager, IEssentials essentials)
        {
            _dataStore = dataStore;
            _recipePackager = recipePackager;
            _essentials = essentials;
            Title = Resources.AllRecipes;
            LoadItemsCommand = new Command(async () => await Refresh());
            AddItemCommand = new Command(async () => await AddItem());
            ItemTappedCommand = new Command<RecipeViewModel>(async (item) => await GoToItemDetail(item));
            FilterListCommand = new Command(async () => await DebouncedSearch());
            SortCommand = new Command(async () => await Sort());

            MessagingCenter.Subscribe<EditItemViewModel>(this, "RecipeChanged", async (item) => await Refresh());
            MessagingCenter.Subscribe<RecipePackager>(this, "RecipesImported", async (item) => await Refresh());
            MessagingCenter.Subscribe<ItemDetailViewModel>(this, "RecipeDeleted", async (item) => await Refresh());
            MessagingCenter.Subscribe<SortItemsViewModel>(this, "SortChanged", async (item) => await Refresh());
        }

        public override async Task InitializeAsync()
        {
            if (Items.Any())
                return; // Already loaded once

            try
            {
                IsRefreshing = true;

                await CreateFirstRecipe();

                await RequestReview();

                await Refresh();

                IsRefreshing = false;

                await ShowReleaseNotes();
            }
            catch (Exception ex)
            {
                await TrackException(ex);
                await DisplayAlertAsync(Resources.ErrorTitle, Resources.EditItemView_UnknownError, Resources.ErrorOk);
            }

            await base.InitializeAsync();
        }

        public void Dispose()
        {
            _throttleCts.Dispose();
        }

        async Task CreateFirstRecipe()
        {
            if (!Items.Any() && _essentials.IsFirstLaunchEver())
                await _recipePackager.CreateDefaultRecipe();
        }

        async Task ShowReleaseNotes()
        {
            if (!_releaseNotesShown && _essentials.IsFirstLaunchForCurrentBuild())
            {
                _releaseNotesShown = true;
                var viewReleaseNotes = await DisplayAlertAsync($"{Resources.ItemsView_WelcomeToVersion} {_essentials.GetVersion()}",
                    Resources.ItemsView_ViewReleaseNotes, Resources.ItemsView_Yes, Resources.ItemsView_No);

                if (viewReleaseNotes)
                    await GoToAsync("about");
            }
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

#pragma warning disable CA1304 // Specify CultureInfo
                var items = string.IsNullOrEmpty(SearchValue)
                    ? await _dataStore.AllAsync<Recipe>()
                    : await _dataStore.QueryAsync<Recipe>(item => item.Title.ToLower().Contains(SearchValue.ToLower()));
#pragma warning restore CA1304 // Specify CultureInfo

                IEnumerable<Recipe> sortedItems;

                var results = _essentials.GetStringSetting("ItemsSortMode") switch
                {
                    "Recent" => items.OrderByDescending(item => item.Id),
                    "Rating" => items.OrderByDescending(item => item.Rating),
                    _ => items.OrderBy(item => item.Title)
                };

                sortedItems = results.ToList();

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

        async Task DebouncedSearch()
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

        async Task Sort()
        {
            await ShowModalAsync("sortItems");
        }

        async Task RequestReview()
        {
            var lastReviewRequest = _essentials.GetStringSetting("LastReviewRequest");
            var nextReview = DateTime.Today.AddDays(15).ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            if (lastReviewRequest == null)
            {
                _essentials.SetStringSetting("LastReviewRequest", nextReview);
                return;
            }

            var today = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            if (string.Compare(lastReviewRequest, today, StringComparison.InvariantCultureIgnoreCase) <= 0)
            {
                _essentials.SetStringSetting("LastReviewRequest", nextReview);
                await _essentials.RequestReview();
            }
        }
    }
}