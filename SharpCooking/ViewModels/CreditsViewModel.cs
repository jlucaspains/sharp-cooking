using SharpCooking.Localization;
using SharpCooking.Models;
using SharpCooking.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public class CreditsViewModel : BaseViewModel
    {
        private readonly IEssentials _essentials;
        private readonly IHttpService _httpService;

        public CreditsViewModel(IEssentials essentials, IHttpService httpService)
        {
            _essentials = essentials;
            _httpService = httpService;

            Title = Resources.CreditsTitle;
            ItemTappedCommand = new Command(async (object model) => await ItemTapped((CreditsItemModel)model));
        }

        public ObservableCollection<CreditsItemModel> Items { get; } = new ObservableCollection<CreditsItemModel>();
        public Command ItemTappedCommand { get; }

        public override async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;

                // Load data
                var data = await _httpService.GetAsync<IEnumerable<CreditsItemModel>>(AppConstants.CreditsDownloadUrl);

                Items.Clear();
                data.ToList().ForEach(Items.Add);

                await base.InitializeAsync();
            }
            catch (Exception ex)
            {
                await TrackException(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ItemTapped(CreditsItemModel model)
        {
            if (string.IsNullOrEmpty(model?.ProjectLink)) return;

            await _essentials.LaunchUri(model.ProjectLink);
        }
    }
}