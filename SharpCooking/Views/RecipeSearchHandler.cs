using PropertyChanged;
using SharpCooking.Data;
using SharpCooking.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharpCooking.Views
{
    [SuppressPropertyChangedWarnings]
    public sealed class RecipeSearchHandler : SearchHandler, IDisposable
    {
        private readonly IDataStore _dataStore;
        private CancellationTokenSource _throttleCts = new CancellationTokenSource();

        public RecipeSearchHandler()
        {
            _dataStore = TinyIoC.TinyIoCContainer.Current.Resolve<IDataStore>();
        }

        protected override async void OnQueryChanged(string oldValue, string newValue)
        {
            await DebouncedSearch(newValue);
        }

        private async Task DebouncedSearch(string search)
        {
            try
            {
                Interlocked.Exchange(ref _throttleCts, new CancellationTokenSource()).Cancel();

                await Task.Delay(TimeSpan.FromMilliseconds(500), _throttleCts.Token)
                    .ContinueWith(async task => await Refresh(search),
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch
            {
                //Ignore any Threading errors
            }
        }

        private async Task Refresh(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                ItemsSource = new System.Collections.Generic.List<RecipeViewModel>();
            }
            else
            {
                var result = await _dataStore.QueryAsync<Recipe>(item => item.Title.ToLower(CultureInfo.CurrentCulture).Contains(search.ToLower(CultureInfo.CurrentCulture)));
                ItemsSource = result.Select(item => RecipeViewModel.FromModel(item)).ToList();
            }
        }

        protected override async void OnItemSelected(object item)
        {
            if(item == null) throw new ArgumentNullException(nameof(item));

            base.OnItemSelected(item);

            await (App.Current.MainPage as Xamarin.Forms.Shell).GoToAsync($"items/detail?id={((RecipeViewModel)item).Id}");
        }

        public void Dispose()
        {
            _throttleCts.Dispose();
        }
    }
}
