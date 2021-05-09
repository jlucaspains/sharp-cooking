using System.Threading.Tasks;
using SharpCooking.Localization;
using SharpCooking.Services;
using Xamarin.Forms;

namespace SharpCooking.ViewModels
{
    public class SortItemsViewModel : BaseViewModel
    {
        private readonly IEssentials _essentials;

        public SortItemsViewModel(IEssentials essentials)
        {
            Title = Resources.SortItems_Title;

            _essentials = essentials;

            SelectSortModeCommand = new Command<string>(async (type) => await ApplySort(type));
        }

        public Command SelectSortModeCommand { get; }

        async Task ApplySort(string type)
        {
            _essentials.SetStringSetting("ItemsSortMode", type);
            MessagingCenter.Send<SortItemsViewModel>(this, "SortChanged");
            await GoBackAsync();
        }
    }
}
